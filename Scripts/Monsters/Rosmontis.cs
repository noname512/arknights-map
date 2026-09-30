using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class Rosmontis : ModMonsterTemplate
{
    

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 120, 120);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 120, 120);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 18);

    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 12);

    private int Damage3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<EmotionExpansionPower>(new ThrowingPlayerChoiceContext(), Creature, 2, Creature, null);
    }

    public int MoveInt = 0;

    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private string GetAttackSfx() => "Attack";

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();

        MoveState attack_summon = new MoveState(
            "ATTACK_SUMMON",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack_A_1", 0.5f);
                await DamageCmd.Attack(Damage1).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                MonsterModel equipment = ModelDb.Monster<RosmontisEquipment>().ToMutable();
                await CreatureCmd.Add(equipment, CombatState, CombatSide.Enemy, CombatState.Encounter!.GetNextSlot(CombatState));
                    await PowerCmd.Apply<MinionPower>(
                        new ThrowingPlayerChoiceContext(),
                        CombatState.Enemies.First(c => c.Monster == equipment),
                        1m,
                        Creature,
                        null);
            },
            [new SingleAttackIntent(Damage1), new SummonIntent()]
        );

        MoveState attack_charge = new MoveState(
            "ATTACK_CHARGE",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack_A_2", 0.5f);
                await DamageCmd.Attack(Damage2).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                
            },
            [new SingleAttackIntent(Damage2)]
        );

        MoveState attack_debuff = new MoveState(
            "ATTACK_DEBUFF",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack_A_3", 0.5f);
                await DamageCmd.Attack(Damage3).WithHitCount(3).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Creature, 10, ValueProp.Unpowered, Creature);
            },
            [new MultiAttackIntent(Damage3, 3)]
        );

        attack_summon.FollowUpState = attack_charge;
        attack_charge.FollowUpState = attack_debuff;
        attack_debuff.FollowUpState = attack_summon;

        list.Add(attack_summon);
        list.Add(attack_charge);
        list.Add(attack_debuff);
        return new MonsterMoveStateMachine(list, attack_summon);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState startState = new AnimState("Start");
        AnimState idleState_A = new AnimState("A_Idle", isLooping: true);
        AnimState attackState_A_1 = new AnimState("A_Attack");
        AnimState attackState_A_2 = new AnimState("A_Attack");
        AnimState attackState_A_3 = new AnimState("A_Skill");

        
        AnimState stunAState = new AnimState("A_Die_Begin");
        AnimState stunloopAState = new AnimState("A_Die_Loop", isLooping: true);
        AnimState stunendAState = new AnimState("A_Die_End");
        AnimState dieAState = new AnimState("A_Die_Begin");

        startState.NextState = idleState_A;
        attackState_A_1.NextState = idleState_A;
        attackState_A_2.NextState = idleState_A;
        attackState_A_3.NextState = idleState_A;
        stunAState.NextState = stunloopAState;
        stunendAState.NextState = idleState_A;
        
        AnimState reviveState = new AnimState("BtoC_2");
        AnimState idleState_C = new AnimState("C_Idle", isLooping: true);
        AnimState attackState_C_1 = new AnimState("C_Attack");
        AnimState attackState_C_2 = new AnimState("C_Attack");
        AnimState attackState_C_3 = new AnimState("B_Skill_Begin");

        
        AnimState stunCState = new AnimState("A_Die_Begin");
        AnimState stunloopCState = new AnimState("A_Die_Loop", isLooping: true);
        AnimState stunendCState = new AnimState("BtoC_2");
        AnimState dieCState = new AnimState("C_Die");

        attackState_C_1.NextState = idleState_C;
        attackState_C_2.NextState = idleState_C;
        attackState_C_3.NextState = idleState_C;
        stunCState.NextState = stunloopCState;
        stunendCState.NextState = idleState_C;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState_A, controller);
        creatureAnimator.AddAnyState("Attack_A_1", attackState_A_1);
        creatureAnimator.AddAnyState("Attack_A_2", attackState_A_2);
        creatureAnimator.AddAnyState("Attack_A_3", attackState_A_3);
        creatureAnimator.AddAnyState("Attack_C_1", attackState_C_1);
        creatureAnimator.AddAnyState("Attack_C_2", attackState_C_2);
        creatureAnimator.AddAnyState("Attack_C_3", attackState_C_3);
        creatureAnimator.AddAnyState("Stun_A", stunAState);
        creatureAnimator.AddAnyState("Stun_A_Loop", stunloopAState);
        creatureAnimator.AddAnyState("Stun_A_End", stunendAState);
        creatureAnimator.AddAnyState("Revive", reviveState);
        creatureAnimator.AddAnyState("Stun_C", stunCState);
        creatureAnimator.AddAnyState("Stun_C_Loop", stunloopCState);
        creatureAnimator.AddAnyState("Stun_C_End", stunendCState);
        creatureAnimator.AddAnyState("Die_A", dieAState);
        creatureAnimator.AddAnyState("Start", startState);
        creatureAnimator.AddAnyState("Die_C", dieCState);

        return creatureAnimator;
    }
}
