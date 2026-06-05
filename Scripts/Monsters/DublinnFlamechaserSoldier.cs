using ArknightsMap.Scripts.Powers;
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
public class DublinnFlamechaserSoldier : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 35, 30);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 35, 30);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);
    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 7);
    public override bool ShouldDisappearFromDoom => !Creature.HasPower<ChaseFlamePower>() || Creature.GetPower<ChaseFlamePower>()?.CurState == 1;
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<ChaseFlamePower>(new ThrowingPlayerChoiceContext(), Creature, 5m, Creature, null);
        if (Creature.SlotName == "third")
        {
            await CreatureCmd.TriggerAnim(Creature, "Idle_2", 0);
            SetMoveImmediate((MoveState)MoveStateMachine.States["STUN3"]);
            Creature.GetPower<ChaseFlamePower>().InitialHp += 1;
            Creature.SetMaxHpInternal(Creature.GetPower<ChaseFlamePower>().InitialHp);
            Creature.GetPower<ChaseFlamePower>().CurState = 1;
            Creature.GetPower<ChaseFlamePower>().MaxHp += Creature.GetPower<ChaseFlamePower>().DecreaseHp;
            Creature.GetPower<ChaseFlamePower>().NextMove = (MoveState)(MoveStateMachine.States["ATTACK1"]);
            Creature.GetPower<ChaseFlamePower>().SetAmount(1);
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets => await DamageCmd.Attack(Damage1).FromMonster(this).WithAttackerAnim("Attack", 0.5f).Execute(null),
            new SingleAttackIntent(Damage1)
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets => await DamageCmd.Attack(Damage2).FromMonster(this).WithAttackerAnim("Attack", 0.5f).Execute(null),
            new SingleAttackIntent(Damage2)
        );
        MoveState attack3 = new MoveState(
            "ATTACK3",
            async targets => await DamageCmd.Attack(Damage1).FromMonster(this).WithAttackerAnim("Attack", 0.5f).Execute(null),
            new SingleAttackIntent(Damage1)
        );
        MoveState stun1 = new MoveState("STUN1", _ => { return Task.CompletedTask; }, new StunIntent());
        MoveState stun2 = new MoveState("STUN2", _ => { return Task.CompletedTask; }, new StunIntent());
        MoveState stun3 = new MoveState("STUN3", async _ =>
        {
            await Creature.GetPower<ChaseFlamePower>()?.Revive();
        }, new HealIntent(), new BuffIntent());

        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack3;
        attack3.FollowUpState = attack1;
        stun1.FollowUpState = stun2;
        stun2.FollowUpState = stun3;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(attack3);
        list.Add(stun1);
        list.Add(stun2);
        list.Add(stun3);

        return new MonsterMoveStateMachine(list, attack1);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        AnimState startState = new AnimState("Start");
        AnimState idleState2 = new AnimState("Idle_2", isLooping: true);
        AnimState dieState2 = new AnimState("Die_2");
        AnimState preReviveState = new AnimState("Die_2");
        AnimState reviveState = new AnimState("Revive");
        attackState.NextState = idleState;
        dieState.NextState = startState;
        startState.NextState = idleState2;
        preReviveState.NextState = reviveState;
        reviveState.NextState = idleState;
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Idle_2", idleState2);
        creatureAnimator.AddAnyState("Revive", dieState);
        creatureAnimator.AddAnyState("Revive2", preReviveState);
        creatureAnimator.AddAnyState("Dead", dieState2);
        creatureAnimator.AddAnyState("Attack", attackState);
        return creatureAnimator;
    }
}