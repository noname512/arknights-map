using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.MonsterMoves;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class DublinnCannoneer : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 45, 42);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 45, 42);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 3);
    private int Times1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);
    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets => await DamageCmd.Attack(Damage1).WithHitCount(Times1).FromMonster(this).WithAttackerAnim("Attack", 0f).OnlyPlayAnimOnce().Execute(null),
            new MultiAttackIntent(Damage1, Times1)
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets => await DamageCmd.Attack(Damage2).FromMonster(this).WithAttackerAnim("Attack", 0.5f).Execute(null),
            new SingleAttackIntent(Damage2)
        );
        MoveState sleep = new MoveState(
            "SLEEP",
            async targets => { },
            new SleepIntent()
        );

        ConditionalBranchState conditionBS = new ConditionalBranchState("CONDITION");
        RandomBranchState randomBS = new RandomBranchState("RANDOM");
        randomBS.AddBranch(attack1, MoveRepeatType.CanRepeatForever);
        randomBS.AddBranch(attack2, MoveRepeatType.CanRepeatForever);
        conditionBS.AddState(randomBS, () => ReedBed.Burning);
        conditionBS.AddState(sleep, () => !ReedBed.Burning);

        attack1.FollowUpState = conditionBS;
        attack2.FollowUpState = conditionBS;
        sleep.FollowUpState = conditionBS;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(sleep);
        list.Add(randomBS);
        list.Add(conditionBS);

        return new MonsterMoveStateMachine(list, conditionBS);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        attackState.NextState = idleState;
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Dead", dieState);
        return creatureAnimator;
    }
}