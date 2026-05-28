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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class Mandragora : ModMonsterTemplate
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 335, 320);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 335, 320);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 45, 40);
    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 4);
    private int HitCount2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int Damage3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 6);
    private int HitCount3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 5);
    private int Damage4 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 27, 25);
    public bool IsHovering { get; set; } = true;
    private int SummonTimes { get; set; } = 0;
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets => await DamageCmd.Attack(Damage1).FromMonster(this).Execute(null),
            new SingleAttackIntent(Damage1)
        );
        MoveState attack2 = new MoveState(
            "GAZE",
            async targets =>
            {
                await DamageCmd.Attack(Damage2).WithHitCount(HitCount2).FromMonster(this).Execute(null);
                await PowerCmd.Apply<MandragoraGazePower>(new ThrowingPlayerChoiceContext(), targets, 4m, Creature, null);
            },
            new MultiAttackIntent(Damage2, HitCount2),
            new DebuffIntent()
        );
        MoveState attack3 = new MoveState(
            "ATTACK2",
            async targets => await DamageCmd.Attack(Damage3).WithHitCount(HitCount3).FromMonster(this).Execute(null),
            new MultiAttackIntent(Damage3, HitCount3)
        );
        MoveState attack4 = new MoveState(
            "ATTACK_DEBUFF",
            async targets =>
            {
                await DamageCmd.Attack(Damage4).FromMonster(this).Execute(null);
                await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, 4m, Creature, null);
            },
            new SingleAttackIntent(Damage4),
            new DebuffIntent()
        );
        MoveState summon = GetSummonState();

        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack3;
        attack3.FollowUpState = attack4;
        attack4.FollowUpState = attack1;
        summon.FollowUpState = attack1;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(attack3);
        list.Add(attack4);
        list.Add(summon);

        return new MonsterMoveStateMachine(list, summon);
    }

    public MoveState GetSummonState()
    {
        if (SummonTimes >= 3)
        {
            return new MoveState(
                "SUMMON" + SummonTimes,
                ShieldAndSummon,
                new StunIntent(),
                new BuffIntent()
            );
        }
        return new MoveState(
            "SUMMON" + SummonTimes,
            ShieldAndSummon,
            new SummonIntent(),
            new BuffIntent()
        );
    }

    public async Task ShieldAndSummon(IReadOnlyList<Creature> targets)
    {
        if (SummonTimes < 3)
        {
            Creature m = await CreatureCmd.Add<TatteredPillar>(CombatState, "second");
            await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), m, 1, Creature, null);
            SummonTimes++;
        }
        await PowerCmd.Apply<StoneshieldPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("C2_Idle_1", isLooping: true);
        AnimState attackState = new AnimState("C2_Attack_1");
        AnimState dieState = new AnimState("C2_Die_1");
        AnimState idleState2 = new AnimState("C2_Idle_2", isLooping: true);
        AnimState attackState2 = new AnimState("C2_Attack_2");
        AnimState dieState2 = new AnimState("C2_Die_2");
        AnimState stunState = new AnimState("C2_Stun");
        attackState.NextState = idleState;
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState, () => IsHovering);
        creatureAnimator.AddAnyState("Attack", attackState2, () => !IsHovering);
        creatureAnimator.AddAnyState("Dead", dieState, () => IsHovering);
        creatureAnimator.AddAnyState("Dead", dieState2, () => !IsHovering);
        return creatureAnimator;
    }
}