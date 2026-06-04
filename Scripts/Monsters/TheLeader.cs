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
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class TheLeader : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 350, 325);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 350, 325);
    private int ScorchingLightNum => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 2, 1);
    private int Damage1_1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 16);
    private int Damage1_2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 1, 1);
    private int Times1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 3);
    private int Damage1_3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);
    private int Damage2_1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 10);
    private int Damage2_2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 2);
    private int Times2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 4);
    private int Damage2_3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 15);
    public bool _isstage2 = false;
    private bool _firstFire2 = true;
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );

    public override async Task AfterAddedToRoom()
    {
        foreach (Creature item in base.Creature.CombatState.GetOpponentsOf(base.Creature))
        {
            GiveAndTakePower giveAndTakePower = (GiveAndTakePower)ModelDb.Power<GiveAndTakePower>().ToMutable();
            giveAndTakePower.Target = item;
            await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), giveAndTakePower, base.Creature, 1, base.Creature, null);
        }
    }

    public async Task Stage1Move()
    {
        if (_isstage2)
        {
            await CreatureCmd.TriggerAnim(Creature, "StartRevive", 0);
        }
        else
        {
            await PowerCmd.Apply<ScorchingLightPower>(new ThrowingPlayerChoiceContext(), Creature, ScorchingLightNum, Creature, null);
        }
    }
    public int GTAmt
    {
        get
        {
            int MaxValue = 0;
            foreach (var power in Creature.Powers)
            {
                if (power is GiveAndTakePower)
                {
                    MaxValue = Math.Max(MaxValue, power.DynamicVars["Exceed"].IntValue);
                }
            }

            return MaxValue;
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState ignite0 = new MoveState(
            "IGNITE0",
            async targets =>
            {
                await Entry.reedBed.SetBurningDurningCombat(true, CombatState);
                await CreatureCmd.TriggerAnim(Creature, "Ignite1", 0);
                await Stage1Move();
            },
            new BuffIntent(),
            new IgniteIntent()
        );
        MoveState attack1_1 = new MoveState(
            "ATTACK1_1",
            async targets =>
            {
                await DamageCmd.Attack(Damage1_1).FromMonster(this).WithAttackerAnim("Attack1", 0.5f).Execute(null);
                await Stage1Move();
            },
            new SingleAttackIntent(Damage1_1),
            new BuffIntent()
        );
        MoveState attack1_2 = new MoveState(
            "ATTACK1_2",
            async targets =>
            {
                await DamageCmd.Attack(Damage1_2).WithHitCount(Times1).FromMonster(this).WithAttackerAnim("Attack1", 0.5f).OnlyPlayAnimOnce().Execute(null);
                await Stage1Move();
            },
            new MultiAttackIntent(Damage1_2, Times1),
            new BuffIntent()
        );
        MoveState ignite1 = new MoveState(
            "IGNITE1",
            async targets =>
            {
                await DamageCmd.Attack(Damage1_3).FromMonster(this).WithAttackerAnim("Ignite1", 0.5f).Execute(null);
                await Entry.reedBed.SetBurningDurningCombat(true, CombatState);
                await Stage1Move();
            },
            new SingleAttackIntent(Damage1_3),
            new BuffIntent(),
            new IgniteIntent()
        );
        MoveState fire1 = new MoveState(
            "RETURN_FIRE1",
            async targets =>
            {
                await DamageCmd.Attack(0).FromMonster(this).WithAttackerAnim("ReturnFire1", 0.5f).Execute(null);
                foreach (var power in Creature.Powers)
                {
                    if (power is GiveAndTakePower giveAndTakePower)
                    {
                        giveAndTakePower.Return(power.DynamicVars["Exceed"].IntValue / 2);
                    }
                }
                await Stage1Move();
            },
            new SingleAttackIntent(0),
            new BuffIntent()
        );

        MoveState attack2_1 = new MoveState(
            "ATTACK2_1",
            async targets => await DamageCmd.Attack(Damage2_1).FromMonster(this).WithAttackerAnim("Attack2", 0.5f).Execute(null),
            new SingleAttackIntent(Damage2_1)
        );
        MoveState attack2_2 = new MoveState(
            "ATTACK2_2",
            async targets => await DamageCmd.Attack(Damage2_2).WithHitCount(Times2).FromMonster(this).WithAttackerAnim("Attack2", 0.5f).OnlyPlayAnimOnce().Execute(null),
            new MultiAttackIntent(Damage2_2, Times2)
        );
        MoveState ignite2 = new MoveState(
            "IGNITE2",
            async targets =>
            {
                await DamageCmd.Attack(Damage2_3).FromMonster(this).WithAttackerAnim("Ignite2", 0.5f).Execute(null);
                await Entry.reedBed.SetBurningDurningCombat(true, CombatState);
            },
            new SingleAttackIntent(Damage2_3),
            new IgniteIntent()
        );
        MoveState fire2 = new MoveState(
            "RETURN_FIRE2",
            async targets =>
            {
                if (_firstFire2)
                {
                    await CreatureCmd.TriggerAnim(Creature, "Revive", 0);
                    _firstFire2 = false;
                }
                else
                {
                    await CreatureCmd.TriggerAnim(Creature, "ReturnFire2", 0);
                }
                int amount = GTAmt;
                await DamageCmd.Attack(0).FromMonster(this).WithNoAttackerAnim().Execute(null);
                foreach (var power in Creature.Powers)
                {
                    if (power is GiveAndTakePower giveAndTakePower)
                    {
                        giveAndTakePower.Return(power.DynamicVars["Exceed"].IntValue);
                    }
                }
            },
            new SingleAttackIntent(0)
        );

        ConditionalBranchState condition1 = new ConditionalBranchState("STAGE1");
        RandomBranchState random1 = new RandomBranchState("RAND1");
        ConditionalBranchState condition2 = new ConditionalBranchState("STAGE2");
        RandomBranchState random2 = new RandomBranchState("RAND2");
        random1.AddBranch(attack1_1, MoveRepeatType.CanRepeatForever);
        random1.AddBranch(attack1_2, MoveRepeatType.CanRepeatForever);
        condition1.AddState(fire2, () => _isstage2);
        condition1.AddState(ignite1, () => !ReedBed.Burning);
        condition1.AddState(fire1, () => GTAmt >= 50);
        condition1.AddState(random1, () => true);
        ignite0.FollowUpState = condition1;
        attack1_1.FollowUpState = condition1;
        attack1_2.FollowUpState = condition1;
        ignite1.FollowUpState = condition1;
        fire1.FollowUpState = condition1;
        condition2.AddState(ignite2, () => !ReedBed.Burning);
        condition2.AddState(fire2, () => GTAmt >= 50);
        condition2.AddState(fire2, () => GTAmt > 0 && RunRng.MonsterAi.NextInt(3) == 1);
        condition2.AddState(random2, () => true);
        random2.AddBranch(attack2_1, MoveRepeatType.CanRepeatForever);
        random2.AddBranch(attack2_2, MoveRepeatType.CanRepeatForever);
        attack2_1.FollowUpState = condition2;
        attack2_2.FollowUpState = condition2;
        ignite2.FollowUpState = condition2;
        fire2.FollowUpState = condition2;

        list.Add(ignite0);
        list.Add(attack1_1);
        list.Add(attack1_2);
        list.Add(attack2_1);
        list.Add(attack2_2);
        list.Add(ignite1);
        list.Add(ignite2);
        list.Add(fire1);
        list.Add(fire2);
        list.Add(random1);
        list.Add(random2);
        list.Add(condition1);
        list.Add(condition2);

        return new MonsterMoveStateMachine(list, ignite0);
    }

    public override async Task AfterCurrentHpChanged(Creature creature, decimal _)
    {
        if (creature != base.Creature) return;
        if (!_isstage2 && Creature.CurrentHp <= Creature.MaxHp - Creature.MaxHp / 2)
        {
            _isstage2 = true;
        }
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState1 = new AnimState("Idle_a", isLooping: true);
        AnimState idleState2 = new AnimState("Idle_b", isLooping: true);
        AnimState attackState1 = new AnimState("Attack_a");
        AnimState attackState2 = new AnimState("Attack_b");
        AnimState igniteState1 = new AnimState("Skill_a_2");
        AnimState igniteState2 = new AnimState("Skill_b_2");
        AnimState fireState1 = new AnimState("Skill_a_1");
        AnimState fireState2 = new AnimState("Skill_b_1");
        AnimState reviveFireState = new AnimState("Skill_b_3");
        AnimState dieState1 = new AnimState("Die");
        AnimState dieState2 = new AnimState("Die_2");
        AnimState dieLoopState = new AnimState("Die_Idle", isLooping: true);
        AnimState reviveState1 = new AnimState("Die_End");
        attackState1.NextState = idleState1;
        attackState2.NextState = idleState2;
        igniteState1.NextState = idleState1;
        igniteState2.NextState = idleState2;
        fireState1.NextState = idleState1;
        fireState2.NextState = idleState2;
        reviveState1.NextState = reviveFireState;
        reviveFireState.NextState = idleState2;
        dieState1.NextState = dieLoopState;
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState1, controller);
        creatureAnimator.AddAnyState("Attack1", attackState1);
        creatureAnimator.AddAnyState("Attack2", attackState2);
        creatureAnimator.AddAnyState("Ignite1", igniteState1);
        creatureAnimator.AddAnyState("Ignite2", igniteState2);
        creatureAnimator.AddAnyState("ReturnFire1", fireState1);
        creatureAnimator.AddAnyState("ReturnFire2", fireState2);
        creatureAnimator.AddAnyState("StartRevive", dieState1);
        creatureAnimator.AddAnyState("Revive", reviveState1);
        creatureAnimator.AddAnyState("Dead", dieState2);
        return creatureAnimator;
    }
}