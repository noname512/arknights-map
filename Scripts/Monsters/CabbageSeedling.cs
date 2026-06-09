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

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class CabbageSeedling : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 40, 38);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 44, 42);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);
    private int Damage3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int Damage4 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 20, 20);
    private bool IsBurning;
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );

    private bool IsBurningVineInCombat() => CombatState.Enemies.Any(e => e.IsAlive && e.IsMonster && e.Monster is BurningVine);

    public override async Task AfterAddedToRoom()
    {
        IsBurning = IsBurningVineInCombat();
        if (IsBurning)
        {
            await PowerCmd.Apply<BurningPower>(new ThrowingPlayerChoiceContext(), Creature, 15m, Creature, null);
        }
    }

    private string GetAttackSfx() => IsBurning ? $"event:/ArknightsMap/sfx/{GetType().Name}/attack" : $"event:/ArknightsMap/sfx/{GetType().Name}/attack_burning";

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
            async targets => await DamageCmd
                .Attack(Damage1)
                .FromMonster(this)
                .WithAttackerAnim("Attack", 0.8f)
                .WithHitFx(sfx: GetAttackSfx())
                .Execute(null),
            new SingleAttackIntent(Damage1)
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets => await DamageCmd
            .Attack(Damage2)
                .FromMonster(this)
                .WithAttackerAnim("Attack", 0.8f)
                .WithHitFx(sfx: GetAttackSfx())
                .Execute(null),
            new SingleAttackIntent(Damage2)
        );
        MoveState attack3 = new MoveState(
            "ATTACK3",
            async targets => await DamageCmd
                .Attack(Damage3)
                .FromMonster(this)
                .WithAttackerAnim("Attack", 0.8f)
                .WithHitFx(sfx: GetAttackSfx())
                .Execute(null),
            new SingleAttackIntent(Damage3)
        );
        MoveState attack4 = new MoveState(
            "ATTACK_DIE",
            async targets =>
            {
                await DamageCmd
                    .Attack(Damage4)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.8f)
                    .WithHitFx(sfx: GetAttackSfx())
                    .Execute(null);
                await PowerCmd.Remove<BurningPower>(Creature);
                await CreatureCmd.Kill(Creature);
            },
            new DeathBlowIntent(() => Damage4)
        );

        RandomBranchState randomBranchState = new RandomBranchState("RAND");
        randomBranchState.AddBranch(attack1, MoveRepeatType.CanRepeatForever);
        randomBranchState.AddBranch(attack2, MoveRepeatType.CanRepeatForever);
        randomBranchState.AddBranch(attack3, MoveRepeatType.CanRepeatForever);
        randomBranchState.AddBranch(attack4, MoveRepeatType.CanRepeatForever, () => IsBurningVineInCombat() ? 1f : 0f);
        attack1.FollowUpState = randomBranchState;
        attack2.FollowUpState = randomBranchState;
        attack3.FollowUpState = randomBranchState;
        attack4.FollowUpState = randomBranchState;
        stun.FollowUpState = randomBranchState;

        list.Add(stun);
        list.Add(attack1);
        list.Add(attack2);
        list.Add(attack3);
        list.Add(attack4);
        list.Add(randomBranchState);

        return new MonsterMoveStateMachine(list, stun);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        if (IsBurning)
        {
            AnimState startState = new AnimState("1to2");
            AnimState idleState = new AnimState("2_Idle", isLooping: true);
            AnimState attackState = new AnimState("2_Attack");
            AnimState dieState = new AnimState("2_Die");
            startState.NextState = idleState;
            attackState.NextState = idleState;
            CreatureAnimator creatureAnimator = new CreatureAnimator(startState, controller);
            creatureAnimator.AddAnyState("Attack", attackState);
            creatureAnimator.AddAnyState("Dead", dieState);
            return creatureAnimator;
        }
        else
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
}