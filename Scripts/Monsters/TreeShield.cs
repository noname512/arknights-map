using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Models.Powers;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class TreeShield : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 180, 168);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 180, 168);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);
    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int Damage3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 10);
    private int Block1 => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 25, 20);
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );

    public override async Task AfterAddedToRoom()
    {
        await CreatureCmd.TriggerAnim(Creature, "Start", 0);
        await PowerCmd.Apply<TauntPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
        await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), Creature, 7m, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets =>
            {
                await DamageCmd.Attack(Damage1).FromMonster(this).WithAttackerAnim("Attack", 0.7f).Execute(null);
                await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), Creature, 4m, Creature, null);
            },
            new SingleAttackIntent(Damage1),
            new BuffIntent()
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets => await DamageCmd.Attack(Damage2).FromMonster(this).WithAttackerAnim("Attack", 0.7f).Execute(null),
            new SingleAttackIntent(Damage2)
        );
        MoveState block1 = new MoveState(
            "BLOCK1",
            async targets => await CreatureCmd.GainBlock(base.Creature, Block1, ValueProp.Move, null),
            new DefendIntent()
        );
        MoveState attack3 = new MoveState(
            "ATTACK3",
            async targets => await DamageCmd.Attack(Damage3).FromMonster(this).WithAttackerAnim("Attack", 0.7f).Execute(null),
            new SingleAttackIntent(Damage3)
        );

        attack1.FollowUpState = attack2;
        attack2.FollowUpState = block1;
        block1.FollowUpState = attack3;
        attack3.FollowUpState = attack1;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(attack3);
        list.Add(block1);

        return new MonsterMoveStateMachine(list, attack1);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        AnimState startState = new AnimState("Start");
        attackState.NextState = idleState;
        startState.NextState = idleState;
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Dead", dieState);
        creatureAnimator.AddAnyState("Start", startState);
        return creatureAnimator;
    }
}