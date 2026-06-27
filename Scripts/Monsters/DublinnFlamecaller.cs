using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
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
public class DublinnFlamecaller : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 120, 108);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 127, 115);
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private int AttackDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 14, 16);
    private int StrengthGain => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 2);
    private MoveState? attack_burning;
    private MoveState? attack_not_burning;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState fire = new MoveState(
            "FIRE",
            async targets =>
            {
                await ModelDb.Singleton<ReedBed>().SetBurningDurningCombat(true, CombatState);
            },
            new IgniteIntent()
        );
        attack_burning = new MoveState(
            "ATTACK_B",
            async targets =>
            {
                await DamageCmd
                    .Attack(AttackDamage)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.5f)
                    .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                    .Execute(null);
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, StrengthGain, Creature, null);
            },
            new SingleAttackIntent(AttackDamage),
            new BuffIntent()
        );
        attack_not_burning = new MoveState(
            "ATTACK_N",
            async targets =>
            {
                await DamageCmd
                    .Attack(AttackDamage)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.5f)
                    .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                    .Execute(null);
                await ModelDb.Singleton<ReedBed>().SetBurningDurningCombat(true, CombatState);
            },
            new SingleAttackIntent(AttackDamage),
            new IgniteIntent()
        );

        ConditionalBranchState attack = new ConditionalBranchState("ATTACK");
        attack.AddState(attack_burning, () => ReedBed.Burning);
        attack.AddState(attack_not_burning, () => !ReedBed.Burning);

        fire.FollowUpState = attack;
        attack_burning.FollowUpState = attack;
        attack_not_burning.FollowUpState = attack;

        list.Add(fire);
        list.Add(attack_burning);
        list.Add(attack_not_burning);
        list.Add(attack);

        return new MonsterMoveStateMachine(list, fire);
    }

    public override async Task OnReedBedStatusChange(bool burning)
    {
        if (NextMove.Id == "ATTACK_B" && !burning)
            SetMoveImmediate(attack_not_burning!);
        else if (NextMove.Id == "ATTACK_N" && burning)
            SetMoveImmediate(attack_burning!);
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
