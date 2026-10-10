using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class Catapult : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 200, 182);
    public override int MaxInitialHp => MinInitialHp;
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private int Dmg1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    private int ExplodeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 25, 23);

    public override async Task AfterAddedToRoom() { }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState throwState = new MoveState(
            "THROW",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "A_Attack", 0);
                await CreatureCmd.Add<Snowball>(CombatState, "" + (CreaturePositions.PositionOfCreature(Creature) - 1));
            },
            new SummonIntent()
        );
        MoveState attackState1 = new MoveState(
            "ATTACK1",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "B_Attack", 0);
                await DamageCmd.Attack(Dmg1).FromMonster(this).WithNoAttackerAnim().Execute(null);
            },
            new SingleAttackIntent(Dmg1)
        );
        MoveState attackState2 = new MoveState(
            "ATTACK2",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "B_Attack", 0);
                await DamageCmd.Attack(Dmg1).FromMonster(this).WithNoAttackerAnim().Execute(null);
            },
            new SingleAttackIntent(Dmg1)
        );
        MoveState attackState3 = new MoveState(
            "ATTACK3",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "A_Attack", 0);
                await DamageCmd.Attack(ExplodeDamage).FromMonster(this).WithNoAttackerAnim().Execute(null);
            },
            new SingleAttackIntent(ExplodeDamage)
        );
        MoveState skillState = new MoveState(
            "SKILL",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "A_Idle", 0);
            },
            new UnknownIntent()
        );

        ConditionalBranchState conditionalBranchState = new ConditionalBranchState("COND");
        conditionalBranchState.AddState(attackState3, () => CreaturePositions.IsBlock(Creature, CombatState.PlayerCreatures.First()));
        conditionalBranchState.AddState(throwState, () => true);

        throwState.FollowUpState = attackState1;
        attackState1.FollowUpState = attackState2;
        attackState2.FollowUpState = skillState;
        attackState3.FollowUpState = attackState1;
        skillState.FollowUpState = conditionalBranchState;

        list.Add(attackState1);
        list.Add(attackState2);
        list.Add(attackState3);
        list.Add(throwState);
        list.Add(skillState);
        list.Add(conditionalBranchState);

        return new MonsterMoveStateMachine(list, throwState);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleStateA = new AnimState("A_Idle", isLooping: true);
        AnimState attackStateA = new AnimState("A_Attack");
        AnimState dieStateA = new AnimState("A_Die");
        AnimState idleStateB = new AnimState("B_Idle", isLooping: true);
        AnimState attackStateB = new AnimState("B_Attack");
        AnimState dieStateB = new AnimState("B_Die");

        attackStateA.NextState = idleStateB;
        attackStateB.NextState = idleStateB;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleStateA, controller);
        creatureAnimator.AddAnyState("A_Attack", attackStateA);
        creatureAnimator.AddAnyState("B_Attack", attackStateB);
        creatureAnimator.AddAnyState("Dead", dieStateA, () => NextMove.StateId == "THROW" || NextMove.StateId == "ATTACK3");
        creatureAnimator.AddAnyState("Dead", dieStateB, () => true);
        creatureAnimator.AddAnyState("A_Idle", idleStateA);

        return creatureAnimator;
    }
}
