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
    private int Dmg2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 6);
    private int Dmg3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<ColdToTheBonePower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    public int count = 0;
    int repeatCount()
    {
        return count + 2;
    }
    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.5f);
                await DamageCmd.Attack(Dmg1).FromMonster(this).WithNoAttackerAnim().Execute(null);
            },
            new SingleAttackIntent(Dmg1)
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.5f);
                await DamageCmd.Attack(Dmg2).WithHitCount(repeatCount()).FromMonster(this).WithNoAttackerAnim().Execute(null);
                count++;
            },
            new MultiAttackIntent(Dmg2, repeatCount)
        );
        MoveState attack3 = new MoveState(
            "ATTACK_BUFF",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.5f);
                await DamageCmd.Attack(Dmg3).FromMonster(this).WithNoAttackerAnim().Execute(null);
                await PowerCmd.Apply<ColdToTheBonePower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
                if (count == 2)
                {
                    ((MoveState)MoveStateMachine.States["ATTACK2"]).FollowUpState = attack1;
                }
            },
            new SingleAttackIntent(Dmg3), new BuffIntent()
        );

        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack3;
        attack3.FollowUpState = attack1;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(attack3);

        return new MonsterMoveStateMachine(list, attack1);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackBeginState = new AnimState("Attack_Begin");
        AnimState attackLoopState = new AnimState("Attack_Loop");
        AnimState attackEndState = new AnimState("Attack_End");
        AnimState dieState = new AnimState("Die");

        attackBeginState.NextState = attackLoopState;
        attackLoopState.NextState = attackEndState;
        attackEndState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackBeginState);
        creatureAnimator.AddAnyState("Die", dieState);

        return creatureAnimator;
    }
}
