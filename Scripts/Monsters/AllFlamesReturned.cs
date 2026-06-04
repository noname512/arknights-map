using ArknightsMap.Scripts.Cards;
using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class AllFlamesReturned : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 72, 66);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 72, 66);
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );

    private int P1AttackDamage => 5;
    private int P1PurpleFlame => 1;
    private int StrengthGain => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);
    private int RevivePurpleFlame => 3;
    private int P2AttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 26, 23);
    private int P2PurpleFlame => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 4);

    private MoveState p1Attack;
    private MoveState DeadState;
    private MoveState DeadState3;
    private MoveState p2Attack;
    private bool _isStage2 = false;

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<RebornPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();

        p1Attack = new MoveState("ATTACK_P1", async targets =>
        {
            await DamageCmd.Attack(P1AttackDamage).FromMonster(this).WithAttackerAnim("Attack", 0.8f).Execute(null);
            await CardPileCmd.AddToCombatAndPreview<PurpleFlame>(targets, PileType.Hand, P1PurpleFlame, null);
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthGain, Creature, null);
        }, new SingleAttackIntent(P1AttackDamage), new CardDebuffIntent(), new BuffIntent());

        DeadState = new MoveState("RESPAWN_MOVE1", async _ => { await CreatureCmd.Heal(Creature, Creature.MaxHp / 3); }, new StunIntent())
        {
            MustPerformOnceBeforeTransitioning = true
        };

        MoveState DeadState2 = new MoveState("RESPAWN_MOVE2", async _ => { await CreatureCmd.Heal(Creature, Creature.MaxHp / 3); }, new StunIntent());

        DeadState3 = new MoveState("RESPAWN_MOVE3", RespawnMove, new HealIntent(), new BuffIntent(), new IgniteIntent());

        p2Attack = new MoveState("ATTACK_P2", async targets =>
        {
            await DamageCmd.Attack(P2AttackDamage).FromMonster(this).WithAttackerAnim("Attack", 0.8f).Execute(null);
            await Entry.reedBed.SetBurningDurningCombat(true, CombatState);
        }, new SingleAttackIntent(P2AttackDamage), new IgniteIntent());
        MoveState breeth = new MoveState("BREETH", async targets =>
        {
            await CreatureCmd.TriggerAnim(Creature, "Skill", 0);
            await CardPileCmd.AddToCombatAndPreview<PurpleFlame>(targets, PileType.Hand, P2PurpleFlame, null);
        }, new CardDebuffIntent());

        p1Attack.FollowUpState = p1Attack;
        DeadState.FollowUpState = DeadState2;
        DeadState2.FollowUpState = DeadState3;

        p2Attack.FollowUpState = breeth;
        breeth.FollowUpState = p2Attack;

        list.Add(p1Attack);
        list.Add(DeadState);
        list.Add(DeadState2);
        list.Add(DeadState3);

        list.Add(p2Attack);
        list.Add(breeth);

        return new MonsterMoveStateMachine(list, p1Attack);
    }

    public async Task TriggerDeadState()
    {
        foreach (var player in CombatState.Players)
        {
            if (player.PlayerCombatState == null)
            {
                continue;
            }
            await CardPileCmd.AddToCombatAndPreview<PurpleFlame>(player.Creature, PileType.Hand, RevivePurpleFlame, null);
        }

        SetMoveImmediate(DeadState, forceTransition: true);
    }


    private async Task RespawnMove(IReadOnlyList<Creature> targets)
    {
        Creature.GetPower<RebornPower>()?.DoRevive();
        await CreatureCmd.Heal(Creature, Creature.MaxHp - Creature.MaxHp / 3 * 2);
        bool hasPurpleFlameRemain = false;
        List<CardModel> purpleFlames = new List<CardModel>();
        foreach (var player in targets)
        {
            if (player.Player?.PlayerCombatState == null)
            {
                continue;
            }

            foreach (CardModel card in player.Player.PlayerCombatState.AllCards)
            {
                if ((card is PurpleFlame) && (card.Pile.Type != PileType.Exhaust))
                {
                    hasPurpleFlameRemain = true;
                    purpleFlames.Add(card);
                }
            }
        }

        if (hasPurpleFlameRemain)
        {
            await CreatureCmd.TriggerAnim(Creature, "Revive1", 0);
            NextMove.FollowUpState = p1Attack;
            foreach (CardModel card in purpleFlames)
            {
                await CardCmd.Exhaust(new ThrowingPlayerChoiceContext(), card);
            }
        }
        else
        {
            _isStage2 = true;
            await CreatureCmd.TriggerAnim(Creature, "Revive2", 0);
            NextMove.FollowUpState = p2Attack;
            await PowerCmd.Remove<RebornPower>(Creature);
            await PowerCmd.Remove<StrengthPower>(Creature);
            await PowerCmd.Apply<FlameBathPower>(new ThrowingPlayerChoiceContext(), Creature, 50, Creature, null);
            await Entry.reedBed.SetBurningDurningCombat(true, CombatState);
        }

    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState1 = new AnimState("A_Idle", isLooping: true);
        AnimState attackState1 = new AnimState("A_Attack");
        AnimState dieState1 = new AnimState("A_Die_Start");
        AnimState dieLoopState = new AnimState("A_Die_Loop");
        AnimState reviveState1 = new AnimState("A_Die_End");
        AnimState reviveState2 = new AnimState("A_Die_End_2");
        AnimState idleState2 = new AnimState("B_Idle", isLooping: true);
        AnimState attackState2 = new AnimState("B_Attack");
        AnimState skillState = new AnimState("B_Skill_1");
        AnimState dieState2 = new AnimState("B_Die");
        attackState1.NextState = idleState1;
        dieState1.NextState = dieLoopState;
        reviveState1.NextState = idleState1;
        reviveState2.NextState = idleState2;
        attackState2.NextState = idleState2;
        skillState.NextState = idleState2;
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState1, controller);
        creatureAnimator.AddAnyState("Attack", attackState1, () => !_isStage2);
        creatureAnimator.AddAnyState("Attack", attackState2, () => _isStage2);
        creatureAnimator.AddAnyState("Dead", dieState1, () => !_isStage2);
        creatureAnimator.AddAnyState("Dead", dieState2, () => _isStage2);
        creatureAnimator.AddAnyState("Revive1", reviveState1);
        creatureAnimator.AddAnyState("Revive2", reviveState2);
        creatureAnimator.AddAnyState("Skill", skillState);
        return creatureAnimator;
    }
}