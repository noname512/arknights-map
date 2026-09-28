using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class Frostdillo : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 325, 317);
    public override int MaxInitialHp => MinInitialHp;
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private int Dmg1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int Dmg2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 24, 22);

    private bool Stunned;

    public override async Task AfterAddedToRoom() { }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attackState1 = new MoveState(
            "ATTACK1",
            async targets =>
            {
                if (Stunned)
                {
                    await CreatureCmd.TriggerAnim(Creature, "Stun_End", 0);
                }
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0);
                await DamageCmd.Attack(Dmg1).FromMonster(this).WithNoAttackerAnim().Execute(null);
            },
            new SingleAttackIntent(Dmg1)
        );
        MoveState attackState2 = new MoveState(
            "ATTACK2",
            async targets =>
            {
                if (Stunned)
                {
                    await CreatureCmd.TriggerAnim(Creature, "Stun_End", 0);
                }
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0);
                await DamageCmd.Attack(Dmg2).FromMonster(this).WithNoAttackerAnim().Execute(null);
            },
            new SingleAttackIntent(Dmg2)
        );

        ConditionalBranchState conditionalBranchState = new ConditionalBranchState("COND");
        conditionalBranchState.AddState(attackState1, () => CombatState.ContainsMonster<Snowcap>());
        conditionalBranchState.AddState(attackState2, () => true);

        attackState1.FollowUpState = conditionalBranchState;
        attackState2.FollowUpState = conditionalBranchState;

        list.Add(attackState1);
        list.Add(attackState2);
        list.Add(conditionalBranchState);

        return new MonsterMoveStateMachine(list, conditionalBranchState);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleStateA = new AnimState("Idle_A", isLooping: true);
        AnimState attackState = new AnimState("Attack_A");
        AnimState dieStateA = new AnimState("Die_A");
        AnimState idleStateB = new AnimState("Idle_B", isLooping: true);
        AnimState dieStateB = new AnimState("Die_B");
        AnimState stunBegin = new AnimState("Die_Begin");
        AnimState stunEnd = new AnimState("Die_End");

        attackState.NextState = idleStateA;
        stunBegin.NextState = idleStateB;
        stunEnd.NextState = idleStateA;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleStateA, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Die", dieStateB, () => Creature.HasPower<WeaknessPower>());
        creatureAnimator.AddAnyState("Die", dieStateA, () => true);
        creatureAnimator.AddAnyState("Stun_Begin", stunBegin);
        creatureAnimator.AddAnyState("Stun_End", stunEnd);

        return creatureAnimator;
    }

    public override async Task OnWindBlow()
    {
        if (!Stunned)
        {
            Stunned = true;
            await CreatureCmd.TriggerAnim(Creature, "Stun_Begin", 0);
        }
        await PowerCmd.Apply<WeaknessPower>(new ThrowingPlayerChoiceContext(), Creature, 325, Creature, null);
    }
}
