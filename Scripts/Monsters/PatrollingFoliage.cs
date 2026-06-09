using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class PatrollingFoliage : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 10, 10);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 10, 10);
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<CaughtOutPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState stun = new MoveState(
            "STUN",
            async targets => { },
            new StunIntent()
        );
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets =>
            {
                await DamageCmd
                    .Attack(7)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.8f)
                    .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                    .Execute(null);
                await PowerCmd.Apply<WeakPower>(
                    new ThrowingPlayerChoiceContext(),
                    targets,
                    AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1),
                    Creature,
                    null);
            },
            new SingleAttackIntent(7),
            new DebuffIntent()
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets => await DamageCmd
                .Attack(4)
                .WithHitCount(2)
                .FromMonster(this)
                .WithAttackerAnim("Attack", 0.7f)
                .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                .OnlyPlayAnimOnce()
                .Execute(null),
            new MultiAttackIntent(4, 2)
        );
        MoveState attack3 = new MoveState(
            "ATTACK3",
            async targets => await DamageCmd
                .Attack(3)
                .WithHitCount(3)
                .FromMonster(this)
                .WithAttackerAnim("Attack", 0.7f)
                .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                .OnlyPlayAnimOnce()
                .Execute(null),
            new MultiAttackIntent(3, 3)
        );
        MoveState attack4 = new MoveState(
            "ATTACK4",
            async targets => await DamageCmd
                .Attack(10)
                .FromMonster(this)
                .WithAttackerAnim("Attack", 0.7f)
                .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                .Execute(null),
            new SingleAttackIntent(10)
        );
        MoveState attack5 = new MoveState(
            "ATTACK5",
            async targets => await DamageCmd
                .Attack(11)
                .FromMonster(this)
                .WithAttackerAnim("Attack", 0.7f)
                .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                .Execute(null),
            new SingleAttackIntent(11)
        );

        RandomBranchState randomBranchState = new RandomBranchState("RAND");
        randomBranchState.AddBranch(attack1, MoveRepeatType.UseOnlyOnce);
        randomBranchState.AddBranch(attack2, MoveRepeatType.UseOnlyOnce);
        randomBranchState.AddBranch(attack3, MoveRepeatType.UseOnlyOnce);
        randomBranchState.AddBranch(attack4, MoveRepeatType.UseOnlyOnce);
        randomBranchState.AddBranch(attack5, MoveRepeatType.UseOnlyOnce);
        attack1.FollowUpState = randomBranchState;
        attack2.FollowUpState = randomBranchState;
        attack3.FollowUpState = randomBranchState;
        attack4.FollowUpState = randomBranchState;
        attack5.FollowUpState = randomBranchState;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(attack3);
        list.Add(attack4);
        list.Add(attack5);
        list.Add(randomBranchState);

        if (AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 1, 0) == 1)
        {
            MoveState attack6 = new MoveState(
                "ATTACK6",
                async targets => await DamageCmd.Attack(3).WithHitCount(5).FromMonster(this).Execute(null),
                new MultiAttackIntent(3, 5)
            );
            randomBranchState.AddBranch(attack6, MoveRepeatType.UseOnlyOnce);
            attack6.FollowUpState = randomBranchState;
            list.Add(attack6);
        }

        return new MonsterMoveStateMachine(list, randomBranchState);
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