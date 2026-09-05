using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Powers;
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
public class FrozenMountainBurdenbeast : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 200, 180);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 220, 200);

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");
    private int Dmg1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 39, 35);
    private int Dmg2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 52, 46);

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<SleepPower>(new ThrowingPlayerChoiceContext(), Creature, 3, Creature, null);
        await PowerCmd.Apply<DamageOutPower>(new ThrowingPlayerChoiceContext(), Creature, 10, Creature, null);
        await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), Creature, 15, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0);
                await DamageCmd.Attack(Dmg1).FromMonster(this).Execute(null);
            },
            new SingleAttackIntent(Dmg1)
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0);
                await CreatureCmd.TriggerAnim(Creature, "Sleep", 0);
                await DamageCmd.Attack(Dmg2).FromMonster(this).Execute(null);
                foreach (PowerModel power in Creature.Powers.ToList())
                {
                    if (power.Type == PowerType.Debuff)
                    {
                        await PowerCmd.Remove(power);
                    }
                }
                await PowerCmd.Apply<SleepPower>(new ThrowingPlayerChoiceContext(), Creature, 3, Creature, null);
                await PowerCmd.Apply<DamageOutPower>(new ThrowingPlayerChoiceContext(), Creature, 10, Creature, null);
                await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), Creature, 15, Creature, null);
            },
            new SingleAttackIntent(Dmg2)
        );
        MoveState sleep = new MoveState(
            "SLEEP",
            async targets =>
            {
                if (Creature.GetPower<SleepPower>()?.Amount == 1)
                {
                    await CreatureCmd.TriggerAnim(Creature, "Awake", 0);
                }
            },
            new SleepIntent()
        );
        MoveState awake = new MoveState(
            "AWAKE",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Awake", 0);
            },
            new StunIntent()
        );

        attack1.FollowUpState = attack2;
        attack2.FollowUpState = sleep;

        ConditionalBranchState conditionalBranchState = new ConditionalBranchState("SLEEP_CHECK");
        conditionalBranchState.AddState(sleep, () => (Creature.GetPower<SleepPower>()?.Amount ?? 0) > 1);
        conditionalBranchState.AddState(attack1, () => true);

        awake.FollowUpState = attack1;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(sleep);
        list.Add(awake);

        return new MonsterMoveStateMachine(list, attack1);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState sleepState = new AnimState("C1_Idle", isLooping: true);
        AnimState awakeState = new AnimState("C1_Revive");
        AnimState idleState = new AnimState("C2_Idle", isLooping: true);
        AnimState attackState = new AnimState("C2_Attack");
        AnimState dieState = new AnimState("C2_Die");

        awakeState.NextState = idleState;
        attackState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(sleepState, controller);
        creatureAnimator.AddAnyState("Awake", awakeState);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Die", dieState);

        return creatureAnimator;
    }
}
