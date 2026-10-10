using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class IcefieldArtsFighter : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 12, 12);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 199, 160);

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");
    private int Dmg1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 13, 11);
    private int Dmg2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);
    private int Dmg3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int StrengthAmt => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<ChainReactionPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        await PowerCmd.Apply<ScatteredToTheWindPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0f);
                await Cmd.Wait(0.97f);
                await DamageCmd.Attack(Dmg1).FromMonster(this).WithNoAttackerAnim().Execute(null);
            },
            new SingleAttackIntent(Dmg1)
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0f);
                await Cmd.Wait(0.97f);
                await DamageCmd.Attack(Dmg2).FromMonster(this).WithNoAttackerAnim().Execute(null);
                await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
            },
            new SingleAttackIntent(Dmg2),
            new DebuffIntent()
        );
        MoveState buff = new MoveState(
            "BUFF",
            async targets =>
            {
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAmt, Creature, null);
            },
            new BuffIntent()
        );
        MoveState attack3 = new MoveState(
            "ATTACK3",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0f);
                await Cmd.Wait(0.97f);
                await DamageCmd.Attack(Dmg3).WithHitCount(2).FromMonster(this).WithNoAttackerAnim().Execute(null);
            },
            new MultiAttackIntent(Dmg3, 2)
        );

        RandomBranchState randomBranchState = new RandomBranchState("RANDOM");
        randomBranchState.AddBranch(attack1, MoveRepeatType.CanRepeatForever, 1);
        randomBranchState.AddBranch(attack2, MoveRepeatType.CanRepeatForever, 1);
        randomBranchState.AddBranch(buff, MoveRepeatType.CanRepeatForever, 1);
        randomBranchState.AddBranch(attack3, MoveRepeatType.CanRepeatForever, 1);
        attack1.FollowUpState = randomBranchState;
        attack2.FollowUpState = randomBranchState;
        buff.FollowUpState = randomBranchState;
        attack3.FollowUpState = randomBranchState;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(buff);
        list.Add(attack3);
        list.Add(randomBranchState);

        return new MonsterMoveStateMachine(list, randomBranchState);
    }

    public override async Task OnWindBlow()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Creature, 200, ValueProp.Unpowered, null, null);
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
