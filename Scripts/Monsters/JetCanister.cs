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
public class JetCanister : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 285, 270);
    public override int MaxInitialHp => MinInitialHp;

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");
    private int Dmg1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 26, 24);
    private int Dmg2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<ColdToTheBonePower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        int repeatCount() => CombatState.RoundNumber;
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
                await DamageCmd.Attack(Dmg2).WithHitCount(repeatCount()).FromMonster(this).Execute(null);
            },
            new MultiAttackIntent(Dmg2, repeatCount)
        );

        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack2;

        list.Add(attack1);
        list.Add(attack2);

        return new MonsterMoveStateMachine(list, attack1);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackBeginState = new AnimState("Attack_Begin");
        AnimState attackIdleState = new AnimState("Attack_Idle");
        AnimState attackEndState = new AnimState("Attack_End");
        AnimState dieState = new AnimState("Die");

        attackBeginState.NextState = attackIdleState;
        attackIdleState.NextState = attackEndState;
        attackEndState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackBeginState);
        creatureAnimator.AddAnyState("Die", dieState);

        return creatureAnimator;
    }
}
