using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class DublinnFlamerazer : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 79, 72);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 79, 72);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 2);
    private int Times1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int Times2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 8);
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );

    private MoveState? attack1_burning;
    private MoveState? attack1_not_burning;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        attack1_burning = new MoveState(
            "ATTACK1_B",
            async targets => await DamageCmd.Attack(Damage1).WithHitCount(Times2).WithNoAttackerAnim().FromMonster(this).WithAttackerAnim("Skill", 0.5f).OnlyPlayAnimOnce().Execute(null),
            new MultiAttackIntent(Damage1, Times2)
        );
        attack1_not_burning = new MoveState(
            "ATTACK1_N",
            async targets =>
            {
                await DamageCmd.Attack(Damage1).WithHitCount(Times1).WithNoAttackerAnim().FromMonster(this).WithAttackerAnim("Skill", 0.5f).OnlyPlayAnimOnce().Execute(null);
                await Entry.reedBed.SetBurningDurningCombat(true, CombatState);
            },
            new MultiAttackIntent(Damage1, Times1),
            new IgniteIntent()
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill", 0);
                await DamageCmd.Attack(Damage1).WithHitCount(Times2).WithNoAttackerAnim().FromMonster(this).WithAttackerAnim("Skill", 0.5f).OnlyPlayAnimOnce().Execute(null);
            },
            new MultiAttackIntent(Damage1, Times2)
        );
        MoveState attack3 = new MoveState(
            "ATTACK3",
            async targets => await DamageCmd.Attack(Damage2).FromMonster(this).WithAttackerAnim("Attack", 0.5f).Execute(null),
            new SingleAttackIntent(Damage2)
        );
        MoveState attack4 = new MoveState(
            "ATTACK4",
            async targets => await DamageCmd.Attack(Damage2).FromMonster(this).WithAttackerAnim("Attack", 0.5f).Execute(null),
            new SingleAttackIntent(Damage2)
        );

        ConditionalBranchState attack1 = new ConditionalBranchState("ATTACK1");
        attack1.AddState(attack1_burning, () => ReedBed.Burning);
        attack1.AddState(attack1_not_burning, () => !ReedBed.Burning);
        attack1_burning.FollowUpState = attack2;
        attack1_not_burning.FollowUpState = attack2;
        attack2.FollowUpState = attack3;
        attack3.FollowUpState = attack4;
        attack4.FollowUpState = attack1;

        list.Add(attack1_burning);
        list.Add(attack1_not_burning);
        list.Add(attack1);
        list.Add(attack2);
        list.Add(attack3);
        list.Add(attack4);

        return new MonsterMoveStateMachine(list, attack1);
    }

    public override async Task OnReedBedStatusChange(bool burning)
    {
        if (NextMove.Id == "ATTACK1_B" && !burning) SetMoveImmediate(attack1_not_burning!);
        else if (NextMove.Id == "ATTACK1_N" && burning) SetMoveImmediate(attack1_burning!);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        AnimState skillStart = new AnimState("Skill_Front_Start");
        AnimState skillLoop = new AnimState("Skill_Front_Loop");
        AnimState skillEnd = new AnimState("Skill_Front_End");
        attackState.NextState = idleState;
        skillStart.NextState = skillLoop;
        skillLoop.NextState = skillEnd;
        skillEnd.NextState = idleState;
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Dead", dieState);
        creatureAnimator.AddAnyState("Skill", skillStart);
        return creatureAnimator;
    }
}