using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class DublinnFlamechaserGuard : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 110, 100);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 110, 100);
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 12);
    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    public override bool ShouldDisappearFromDoom => false;

    private MoveState? buff_burning;
    private MoveState? buff_not_burning;

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<ChaseFlamePower>(new ThrowingPlayerChoiceContext(), Creature, 10, Creature, null);
        // await PowerCmd.Apply<FlameBathPower>(new ThrowingPlayerChoiceContext(), Creature, 50, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets => await DamageCmd
                .Attack(Damage1)
                .FromMonster(this)
                .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}/attack")
                .Execute(null),
            new SingleAttackIntent(Damage1)
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets => await DamageCmd
                .Attack(Damage2)
                .FromMonster(this)
                .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}/attack")
                .Execute(null),
            new SingleAttackIntent(Damage2)
        );
        buff_burning = new MoveState(
            "BUFF_B",
            async targets => await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 12, Creature, null),
            new BuffIntent()
        );
        buff_not_burning = new MoveState(
            "BUFF_N",
            async targets =>
            {
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 2, Creature, null);
                await Entry.reedBed.SetBurningDurningCombat(true, CombatState);
            },
            new BuffIntent(), new IgniteIntent()
        );
        MoveState stun1 = new MoveState("STUN1", _ => { return Task.CompletedTask; }, new StunIntent());
        MoveState stun3 = new MoveState("STUN3", async _ =>
        {
            SfxCmd.Play($"event:/ArknightsMap/sfx/{GetType().Name}/reborn");
            await (Creature.GetPower<ChaseFlamePower>()?.Revive() ?? Task.CompletedTask);
            await Entry.reedBed.SetBurningDurningCombat(true, CombatState);
        }, new HealIntent(), new IgniteIntent());
        // 不要改，复活的意图就是STUN3，改了可能会炸

        ConditionalBranchState buff = new ConditionalBranchState("BUFF");
        buff.AddState(buff_burning, () => ReedBed.Burning);
        buff.AddState(buff_not_burning, () => !ReedBed.Burning);

        attack1.FollowUpState = attack2;
        attack2.FollowUpState = buff;
        buff_burning.FollowUpState = attack1;
        buff_not_burning.FollowUpState = attack1;
        stun1.FollowUpState = stun3;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(buff_burning);
        list.Add(buff_not_burning);
        list.Add(buff);
        list.Add(stun1);
        list.Add(stun3);

        return new MonsterMoveStateMachine(list, attack1);
    }

    public override async Task OnReedBedStatusChange(bool burning)
    {
        if (NextMove.Id == "BUFF_B" && !burning) SetMoveImmediate(buff_not_burning!);
        else if (NextMove.Id == "BUFF_N" && burning) SetMoveImmediate(buff_burning!);
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
        creatureAnimator.AddAnyState("Revive", dieState);
        creatureAnimator.AddAnyState("Revive2", preReviveState);
        creatureAnimator.AddAnyState("Dead", dieState2);
        creatureAnimator.AddAnyState("Attack", attackState);
        return creatureAnimator;
    }
}