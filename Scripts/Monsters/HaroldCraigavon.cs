using ArknightsMap.Scripts.Cards;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class HaroldCraigavon : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 505, 460);
    public override int MaxInitialHp => MinInitialHp;

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");
    private int dmg1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 65, 53);
    private int dmg2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 130, 106);
    private int dmg3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 55, 44);
    private int dmg4 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 80, 67);
    private int cardNum => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<StandStillPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState start = new MoveState(
            "START",
            async targets =>
            {
                await CardPileCmd.AddToCombatAndPreview<EmergencyHeater>(targets, PileType.Hand, cardNum, null);
            },
            new CardDebuffIntent()
        );
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.5f);
                await DamageCmd.Attack(dmg1).FromMonster(this).WithNoAttackerAnim().Execute(null);
                await CreatureCmd.TriggerAnim(Creature, "Move", 0.5f);
                await CreaturePositions.Walk(Creature, -1);
            },
            new SingleAttackIntent(dmg1),
            new MoveIntent()
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack_2", 0.5f);
                await DamageCmd.Attack(dmg2).FromMonster(this).WithNoAttackerAnim().Execute(null);
            },
            new SingleAttackIntent(dmg2)
        );
        MoveState attackDebuff = new MoveState(
            "ATTACK_DEBUFF",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill_1", 0.5f);
                await DamageCmd.Attack(dmg3).FromMonster(this).WithNoAttackerAnim().Execute(null);
                foreach (Creature c in targets)
                {
                    if (c.IsPlayer)
                    {
                        Player p = c.Player!;
                        var hand = p.PlayerCombatState!.Hand.Cards;
                        List<CardModel> cards = hand.Where(c => c is EmergencyHeater).ToList();
                        if (cards.Any())
                        {
                            CardModel card = cards.TakeRandom(1, p.RunState.Rng.CombatCardSelection).First();
                            List<CardModel> target = [card];
                            int index = hand.IndexOf(card);
                            if (index > 0 && hand[index - 1] is EmergencyHeater)
                            {
                                target.Add(hand[index - 1]);
                            }
                            if (index < hand.Count - 1 && hand[index + 1] is EmergencyHeater)
                            {
                                target.Add(hand[index + 1]);
                            }
                            foreach (CardModel item in target)
                            {
                                await CardCmd.TransformTo<EstinguishedEmergencyHeater>(item);
                            }
                        }
                    }
                }
            },
            new SingleAttackIntent(dmg3),
            new DebuffIntent()
        );
        MoveState skill = new MoveState(
            "SKILL",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill_2", 0.5f);
                await Cmd.Wait(1f);
                await DamageCmd.Attack(dmg4).FromMonster(this).WithHitCount(CalcRepeatTimes()).WithNoAttackerAnim().Execute(null);
            },
            new MultiAttackIntent(dmg4, CalcRepeatTimes)
        );

        ConditionalBranchState attackBranchState = new ConditionalBranchState("ATTACKS");
        attackBranchState.AddState(attack2, () => CreaturePositions.IsBlock(Creature, CombatState.PlayerCreatures.First()));
        attackBranchState.AddState(attack1, () => true);

        RandomBranchState randomBranchState = new RandomBranchState("RANDOM");
        randomBranchState.AddBranch(attackBranchState, 1);
        randomBranchState.AddBranch(attackDebuff, 1);

        ConditionalBranchState conditionalBranchState = new ConditionalBranchState("MOVES");
        conditionalBranchState.AddState(skill, () => CombatState.RoundNumber % 6 == 0);
        conditionalBranchState.AddState(randomBranchState, () => true);

        start.FollowUpState = conditionalBranchState;
        attack1.FollowUpState = conditionalBranchState;
        attack2.FollowUpState = conditionalBranchState;
        attackDebuff.FollowUpState = conditionalBranchState;
        skill.FollowUpState = conditionalBranchState;

        list.Add(start);
        list.Add(skill);
        list.Add(attack1);
        list.Add(attack2);
        list.Add(attackDebuff);
        list.Add(attackBranchState);
        list.Add(randomBranchState);
        list.Add(conditionalBranchState);

        return new MonsterMoveStateMachine(list, start);
    }

    public int CalcRepeatTimes()
    {
        int times = 1;
        Player? me = LocalContext.GetMe(CombatState);
        if (me != null)
        {
            times += me.PlayerCombatState!.Hand.Cards.Count(c => c is EstinguishedEmergencyHeater);
            times += me.PlayerCombatState.DrawPile.Cards.Count(c => c is EstinguishedEmergencyHeater);
            times += me.PlayerCombatState.DiscardPile.Cards.Count(c => c is EstinguishedEmergencyHeater);
        }
        return times;
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("B_Idle", isLooping: true);
        AnimState attackState = new AnimState("B_Attack");
        AnimState attack2State = new AnimState("B_Attack_2");
        AnimState moveState = new AnimState("B_Move");
        AnimState skill1State = new AnimState("B_Skill_1");
        AnimState skill2BeginState = new AnimState("B_Skill_2_Begin");
        AnimState skill2LoopState = new AnimState("B_Skill_2_Loop");
        AnimState skill2EndState = new AnimState("B_Skill_2_End");
        AnimState dieState = new AnimState("B_Die");

        attackState.NextState = idleState;
        attack2State.NextState = idleState;
        skill1State.NextState = idleState;
        skill2BeginState.NextState = skill2LoopState;
        skill2LoopState.NextState = skill2EndState;
        skill2EndState.NextState = idleState;
        moveState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Attack_2", attack2State);
        creatureAnimator.AddAnyState("Skill_1", skill1State);
        creatureAnimator.AddAnyState("Skill_2", skill2BeginState);
        creatureAnimator.AddAnyState("Move", moveState);
        creatureAnimator.AddAnyState("Die", dieState);

        return creatureAnimator;
    }
}
