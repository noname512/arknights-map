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
using Godot;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Singleton;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class Nest : ModMonsterTemplate
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 10, 10);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 10, 10);
    private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 2);
    private int StrengthAdd => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 3, 2);
    private int HpAdd => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 12);
    private int growthTimes = 0;
    private int FirstGrow = 1;
    private int SecondGrow = AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 5);

    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );

    public override async Task AfterAddedToRoom()
    {
        if (!CombatState.ContainsMonster<TreeShield>()) SwitchIntent();
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets =>
            {
                await DamageCmd.Attack(Damage).FromMonster(this).Execute(null);
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthAdd, Creature, null);
                await CreatureCmd.GainMaxHp(Creature, HpAdd * CombatState.Players.Count * MultiplayerScalingModel.GetMultiplayerScaling(CombatState.Encounter, CombatState.RunState.CurrentActIndex));
                growthTimes++;
                if (growthTimes == FirstGrow)
                {
                    NCombatRoom.Instance?.GetCreatureNode(Creature)?.ScaleTo(0.5f, 0);
                    await CreatureCmd.TriggerAnim(Creature, "Bigger", 0);
                }
                if (growthTimes == SecondGrow) await CreatureCmd.TriggerAnim(Creature, "Crack", 0);
                if ((growthTimes < SecondGrow) && (growthTimes > FirstGrow))
                {
                    NCombatRoom.Instance?.GetCreatureNode(Creature)?.ScaleTo(0.5f + 0.5f * (growthTimes - FirstGrow) / (SecondGrow - FirstGrow - 1), 0.75);
                }
            },
            new SingleAttackIntent(Damage),
            new BuffIntent(), new HealIntent()
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets => await DamageCmd.Attack(Damage).FromMonster(this).Execute(null),
            new SingleAttackIntent(Damage)
        );

        attack1.FollowUpState = attack1;
        attack2.FollowUpState = attack2;

        list.Add(attack1);
        list.Add(attack2);

        return new MonsterMoveStateMachine(list, attack1);
    }

    public void SwitchIntent()
    {
        SetMoveImmediate((MoveState)MoveStateMachine.States["ATTACK2"], true);
    }

    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature.Monster is TreeShield && CombatState.Enemies.Count(e => e.IsAlive && e.Monster is TreeShield) == 0) SwitchIntent();
        return Task.CompletedTask;
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        AnimState biggerState = new AnimState("Bigger");
        AnimState idleState2 = new AnimState("Idle2", isLooping: true);
        AnimState attackState2 = new AnimState("Attack2");
        AnimState dieState2 = new AnimState("Die2");
        AnimState crackState = new AnimState("Crack");
        AnimState idleState3 = new AnimState("Idle3", isLooping: true);
        AnimState attackState3 = new AnimState("Attack3");
        AnimState dieState3 = new AnimState("Die3");
        attackState.NextState = idleState;
        attackState2.NextState = idleState2;
        attackState3.NextState = idleState3;
        biggerState.NextState = idleState2;
        crackState.NextState = idleState3;
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState, () => growthTimes < FirstGrow);
        creatureAnimator.AddAnyState("Attack", attackState2, () => growthTimes >= FirstGrow && growthTimes < SecondGrow);
        creatureAnimator.AddAnyState("Attack", attackState3, () => growthTimes >= SecondGrow);
        creatureAnimator.AddAnyState("Dead", dieState, () => growthTimes < FirstGrow);
        creatureAnimator.AddAnyState("Dead", dieState2, () => growthTimes >= FirstGrow && growthTimes < SecondGrow);
        creatureAnimator.AddAnyState("Dead", dieState3, () => growthTimes >= SecondGrow);
        creatureAnimator.AddAnyState("Bigger", biggerState);
        creatureAnimator.AddAnyState("Crack", crackState);
        return creatureAnimator;
    }
}