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

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class BurningVine : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 80, 72);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 80, 72);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 22, 20);
    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 13);
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<DealFlamingDamagePower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets => await DamageCmd.Attack(Damage1).FromMonster(this).Execute(null),
            new SingleAttackIntent(Damage1)
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets => await DamageCmd.Attack(Damage2).FromMonster(this).Execute(null),
            new SingleAttackIntent(Damage2)
        );
        MoveState attack3 = new MoveState(
            "ATTACK3",
            async targets => await DamageCmd.Attack(Damage2).FromMonster(this).Execute(null),
            new SingleAttackIntent(Damage2)
        );
        MoveState summon = new MoveState(
            "SUMMON",
            async targets => await CreatureCmd.Add<CabbageSeedling>(CombatState, "second"),
            new SummonIntent()
        );

        ConditionalBranchState conditionalBranchState = new ConditionalBranchState("SUMMON?");
        conditionalBranchState.AddState(summon, () => !CombatState.ContainsMonster<CabbageSeedling>());
        conditionalBranchState.AddState(attack3, () => CombatState.ContainsMonster<CabbageSeedling>());
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = conditionalBranchState;
        summon.FollowUpState = attack3;
        attack3.FollowUpState = attack2;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(attack3);
        list.Add(summon);
        list.Add(conditionalBranchState);

        return new MonsterMoveStateMachine(list, attack1);
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