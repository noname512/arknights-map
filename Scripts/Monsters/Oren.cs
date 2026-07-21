using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class Oren : AbstractSankta
{
    
    protected override int BulletMax => 1;
    protected override int InitialBullet => 0;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 250, 250);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 250, 250);
    private int Damage_Skill => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 25, 22);

    private int Damage_2_Hit => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);
    
    private int Damage_1_Hit => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    
    private int Block => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);
    
    private bool IsTimeIncrease = false;

    
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private bool IsBurningVineInCombat() => CombatState.Enemies.Any(e => e.IsAlive && e.IsMonster && e.Monster is BurningVine);


    public override Task AfterDamageReceivedLate(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == Creature && result.UnblockedDamage > 0)
        {
            CardPileCmd.AddToCombatAndPreview<Dazed>(target, PileType.Draw, 1, null, CardPilePosition.Top);
        }
        return base.AfterDamageReceivedLate(choiceContext, target, result, props, dealer, cardSource);
    }
    

    private string GetAttackSfx() => "Attack";

    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState gainbullet = new MoveState(
            "GAIN_BULLET",
            async targets =>
            {
                await Cmd.Wait(1.0f);
                await AddBullet(1);
            },
            new BuffIntent()
        );
        

        MoveState attackSkill = new MoveState(
            "ATTACK_SKILL",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack_B", 0.8f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(Damage_Skill).FromMonster(this).WithNoAttackerAnim().WithHitFx(sfx: GetAttackSfx()).Execute(null);
                await UseBullet(1);
                foreach(Creature c in targets)
                {
                    await CardPileCmd.AddToCombatAndPreview<Dazed>(c, PileType.Draw, 5, null, CardPilePosition.Top);
                }
            },
            [new SingleAttackIntent(Damage_Skill), new StatusIntent(5)]

        );
        MoveState attack2Hit = new MoveState(
            "ATTACK_2_HIT",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack_A", 0.8f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(Damage_2_Hit).WithHitCount(2).FromMonster(this).WithNoAttackerAnim().WithHitFx(sfx: GetAttackSfx()).Execute(null);
                foreach(Creature c in targets)
                {
                    await CardPileCmd.AddToCombatAndPreview<Dazed>(c, PileType.Discard, 2, null, CardPilePosition.Top);
                }
            },
            [new MultiAttackIntent(Damage_2_Hit, 2), new StatusIntent(2)]
        );

        MoveState attack1Hit = new MoveState(
            "ATTACK_1_HIT",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack_A", 0.8f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(Damage_1_Hit).FromMonster(this).WithNoAttackerAnim().WithHitFx(sfx: GetAttackSfx()).Execute(null);
                foreach(Creature c in targets)
                {
                    
                }
            },
            [new SingleAttackIntent(Damage_1_Hit), new DebuffIntent()]
        );

        MoveState debuff = new MoveState(
            "DEBUFF",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack_A", 0.8f);
                await Cmd.Wait(1.0f);
                await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, 2, Creature, null);
                await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, 2, Creature, null);
                
            },
            [new DebuffIntent()]
        );



        ConditionalBranchState attackBranch1 = new ConditionalBranchState("ATTACK_BRANCH1");
    attackBranch1.AddState(attackSkill, () => Bullet > 0);
    attackBranch1.AddState(attack2Hit, () => Bullet <= 0);

    ConditionalBranchState attackBranch2 = new ConditionalBranchState("ATTACK_BRANCH2");
    attackBranch2.AddState(attackSkill, () => Bullet > 0);
    attackBranch2.AddState(attack1Hit, () => Bullet <= 0);

    ConditionalBranchState attackBranch3 = new ConditionalBranchState("ATTACK_BRANCH3");
    attackBranch3.AddState(attackSkill, () => Bullet > 0);
    attackBranch3.AddState(debuff, () => Bullet <= 0);


        list.Add(gainbullet);
        list.Add(attackSkill);
        list.Add(debuff);
        list.Add(attack2Hit);
        list.Add(attack1Hit);
        list.Add(attackBranch1);
        list.Add(attackBranch2);
        

        gainbullet.FollowUpState = attackSkill;
        attackSkill.FollowUpState = debuff;
        debuff.FollowUpState = attackBranch1;
        attack2Hit.FollowUpState = attackBranch2;
        attack1Hit.FollowUpState = attackBranch3;
        return new MonsterMoveStateMachine(list, gainbullet);
    }

    

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attack01State = new AnimState("Attack_B");
        AnimState attack02State = new AnimState("Attack_A");

        AnimState dieState = new AnimState("Die");
        AnimState skillState = new AnimState("Skill");

        attack01State.NextState = idleState;
        attack02State.NextState = idleState;
        
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack_B", attack01State);
        creatureAnimator.AddAnyState("Attack_A", attack02State);
        creatureAnimator.AddAnyState("Skill", skillState);
        creatureAnimator.AddAnyState("Die", dieState);
        
        return creatureAnimator;
    }
}