using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class TombkeeperGrotesque : ModMonsterTemplate
{
    private bool _isStage2 = false;
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);
    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    private int Status2 => 2;
    private int Damage3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);
    private int HitCount3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 4);
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 105, 100);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 110, 105);
    public override bool ShouldDisappearFromDoom => _isStage2;

    private MoveState _deadState;
    private MoveState DeadState
    {
        get
        {
            return _deadState;
        }
        set
        {
            AssertMutable();
            _deadState = value;
        }
    }
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<RebornPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        DeadState = new MoveState("RESPAWN_MOVE", RespawnMove, new HealIntent(), new BuffIntent())
        {
            MustPerformOnceBeforeTransitioning = true
        };
        MoveState Respawn2 = new MoveState("RESPAWN_2", _ => { return Task.CompletedTask; }, new StunIntent());
        MoveState Respawn3 = new MoveState("RESPAWN_3", _ => { return Task.CompletedTask; }, new StunIntent());
        MoveState Respawn4 = new MoveState("RESPAWN_4", async _ =>
        {
            await CreatureCmd.TriggerAnim(Creature, "Start", 0);
            await PowerCmd.Remove<DamageOutPower>(Creature);
            await PowerCmd.Apply<SoarPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 4m, Creature, null);
            _isStage2 = true;
        }, new BuffIntent());
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets => await DamageCmd.Attack(Damage1).FromMonster(this).Execute(null),
            new SingleAttackIntent(Damage1)
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets =>
            {
                await DamageCmd.Attack(Damage2).FromMonster(this).Execute(null);
                await CardPileCmd.AddToCombatAndPreview<Dazed>(targets, PileType.Discard, Status2, null);
            },
            new SingleAttackIntent(Damage2),
            new StatusIntent(Status2)
        );
        MoveState attack3 = new MoveState(
            "ATTACK3",
            async targets => await DamageCmd.Attack(Damage3).WithHitCount(HitCount3).FromMonster(this).Execute(null),
            new MultiAttackIntent(Damage3, HitCount3)
        );
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack3;
        attack3.FollowUpState = attack1;
        DeadState.FollowUpState = Respawn2;
        Respawn2.FollowUpState = Respawn3;
        Respawn3.FollowUpState = Respawn4;
        Respawn4.FollowUpState = attack1;
        list.Add(DeadState);
        list.Add(Respawn2);
        list.Add(Respawn3);
        list.Add(Respawn4);
        list.Add(attack1);
        list.Add(attack2);
        list.Add(attack3);

        return new MonsterMoveStateMachine(list, attack1);
    }

    private async Task RespawnMove(IReadOnlyList<Creature> targets)
    {
        Creature.GetPower<RebornPower>()?.DoRevive();
        await CreatureCmd.Heal(Creature, Creature.MaxHp);
        await PowerCmd.Apply<DamageOutPower>(new ThrowingPlayerChoiceContext(), Creature, 10, Creature, null);
        await PowerCmd.Remove<RebornPower>(Creature);
        Creature m = base.CombatState.Enemies.FirstOrDefault(m => m.Monster is TatteredPillar, null);
        if (m != null)
        {
            await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), m, 1, Creature, null);
        }
    }

    public void TriggerDeadState()
    {
        SetMoveImmediate(DeadState, forceTransition: true);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        AnimState sleepState = new AnimState("Sleep");
        AnimState startState = new AnimState("Start");
        AnimState idleState2 = new AnimState("Idle_2", isLooping: true);
        AnimState attackState2 = new AnimState("Attack_2");
        AnimState dieState2 = new AnimState("Die_2");
        attackState.NextState = idleState;
        dieState.NextState = sleepState;
        startState.NextState = idleState2;
        attackState2.NextState = idleState2;
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState, () => !_isStage2);
        creatureAnimator.AddAnyState("Attack", attackState2, () => _isStage2);
        creatureAnimator.AddAnyState("Dead", dieState, () => !_isStage2);
        creatureAnimator.AddAnyState("Dead", dieState2, () => _isStage2);
        creatureAnimator.AddAnyState("Start", startState);
        return creatureAnimator;
    }
}