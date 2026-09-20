using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class EggcradlerFowlbeast : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 52, 47);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 62, 57);

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");
    private int dmg1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int dmg2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);

    private bool blowed = false;

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<EggGuardingPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attackDebuff = new MoveState(
            "ATTACK_DEBUFF",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.5f);
                await DamageCmd.Attack(dmg1).FromMonster(this).WithNoAttackerAnim().Execute(null);
                await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
            },
            new SingleAttackIntent(dmg1),
            new DebuffIntent()
        );
        MoveState attack = new MoveState(
            "ATTACK",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.5f);
                await DamageCmd.Attack(dmg2).FromMonster(this).WithNoAttackerAnim().Execute(null);
            },
            new SingleAttackIntent(dmg2)
        );
        MoveState violent = new MoveState(
            "VIOLENT",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.5f);
                await DamageCmd.Attack(dmg2).WithHitCount(2).FromMonster(this).WithNoAttackerAnim().Execute(null);
                await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, 2, Creature, null);
            },
            new MultiAttackIntent(dmg2, 2),
            new DebuffIntent()
        );

        ConditionalBranchState conditionalBranchState = new ConditionalBranchState("CONDITION");
        conditionalBranchState.AddState(violent, () => blowed);
        conditionalBranchState.AddState(attack, () => CombatState.PlayerCreatures.FirstOrDefault()!.HasPower<WeakPower>());
        conditionalBranchState.AddState(attackDebuff, () => true);

        attackDebuff.FollowUpState = conditionalBranchState;
        attack.FollowUpState = conditionalBranchState;
        violent.FollowUpState = conditionalBranchState;

        list.Add(attackDebuff);
        list.Add(attack);
        list.Add(violent);
        list.Add(conditionalBranchState);

        return new MonsterMoveStateMachine(list, conditionalBranchState);
    }

    public override Task OnWindBlow()
    {
        blowed = true;
        return Task.CompletedTask;
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle_B", isLooping: true);
        AnimState attackState = new AnimState("Attack_B");
        AnimState dieState = new AnimState("Die_B");

        attackState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Die", dieState);

        return creatureAnimator;
    }
}
