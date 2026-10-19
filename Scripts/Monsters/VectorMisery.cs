using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class VectorMisery : ModMonsterTemplate
{
    

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 120, 120);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 120, 120);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 24, 25);

    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 15);

    private int Damage3 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);

    public override async Task AfterAddedToRoom()
    {
        
    }

    public NCreature MiseryPosition
    {
        get
        {
            var instance = NCombatRoom.Instance ?? throw new InvalidOperationException("Combat room instance is not available.");
            return instance.GetCreatureNode(Creature) ?? throw new InvalidOperationException("Creature node is not available.");
        }
    }

    public NCreature PlayerPosition
    {
        get
        {
            var instance = NCombatRoom.Instance ?? throw new InvalidOperationException("Combat room instance is not available.");
            return instance.GetCreatureNode(CombatState.GetOpponentsOf(Creature)[0]) ?? throw new InvalidOperationException("Creature node is not available.");
        }
    }

    public bool OnRight => PlayerPosition.Position.X < MiseryPosition.Position.X;

    public bool Dark = false;

    public override async Task AfterDamageReceivedLate(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (result.UnblockedDamage > 0 
        && props.IsPoweredAttack() 
        && cardSource is not null 
        && target == Creature)
        {
            await PowerCmd.Apply<MiseryPower>(new ThrowingPlayerChoiceContext(), target, 1m, Creature, null);
        }
    }



    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private string GetAttackSfx() => "Attack";

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();

        MoveState attack_steal = new MoveState(
            "ATTACK_STEAL",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "A_Skill_2_Begin", 0.5f);
                await DamageCmd.Attack(Damage1).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                foreach (Creature c in CombatState.GetOpponentsOf(Creature))
                {
                    if (c.Player is not null)
                    {
                        await PowerCmd.Apply<HurtPower>(new ThrowingPlayerChoiceContext(), c, 1m, Creature, null);
                    }
                }
            },
            [new SingleAttackIntent(Damage1), new DebuffIntent()]
        );

        MoveState attack_debuff = new MoveState(
            "ATTACK_DEBUFF",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack_A", 0.5f);
                await DamageCmd.Attack(Damage2).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                foreach (Creature c in CombatState.GetOpponentsOf(Creature))
                {
                    if (c.Player is not null)
                    {
                        await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), c, 2m, Creature, null);
                        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), c, 2m, Creature, null);
                        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), c, 2m, Creature, null);
                    }
                }
            },
            [new SingleAttackIntent(Damage1), new DebuffIntent()]
        );

        MoveState debuff = new MoveState(
            "DEBUFF",
            async targets =>
            {
                foreach (Creature c in CombatState.GetOpponentsOf(Creature))
                {
                    if (c.Player is not null)
                    {
                        await PowerCmd.Apply<MiseryDamagePower>(new ThrowingPlayerChoiceContext(), c, 8m, Creature, null);
                    }
                }
            },
            [new DebuffIntent()]
        );

        

        attack_steal.FollowUpState = debuff;
        
        debuff.FollowUpState = attack_debuff;
        attack_debuff.FollowUpState = debuff;

        list.Add(attack_steal);
        list.Add(attack_debuff);
        list.Add(debuff);
        
        return new MonsterMoveStateMachine(list, attack_steal);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState startState = new AnimState("Start");
        AnimState idleState_A = new AnimState("A_Idle", isLooping: true);
        AnimState attackState_A_1 = new AnimState("A_Attack");
        AnimState attackState_A_2 = new AnimState("A_Attack");
        AnimState skillState_A = new AnimState("A_Skill_2_Begin");
        AnimState skillState_A_2 = new AnimState("A_Skill_2_End");

        
        AnimState retreatBeginState = new AnimState("B_Skill_3_Retreat_Begin");
        AnimState retreatEndState = new AnimState("B_Skill_3_Retreat_End");
        
        AnimState dieState = new AnimState("B_Die");

        startState.NextState = idleState_A;
        attackState_A_1.NextState = idleState_A;
        attackState_A_2.NextState = idleState_A;
        skillState_A.NextState = skillState_A_2;
        skillState_A_2.NextState = idleState_A;
        retreatBeginState.NextState = retreatEndState;
        retreatEndState.NextState = idleState_A;


        
       

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState_A, controller);
        creatureAnimator.AddAnyState("A_Attack", attackState_A_1);
        creatureAnimator.AddAnyState("Attack_A_2", attackState_A_2);
        creatureAnimator.AddAnyState("A_Skill_2_Begin", skillState_A);
        creatureAnimator.AddAnyState("A_Skill_2_End", skillState_A_2);
        creatureAnimator.AddAnyState("B_Skill_3_Retreat_Begin", retreatBeginState);
        creatureAnimator.AddAnyState("B_Skill_3_Retreat_End", retreatEndState);

        creatureAnimator.AddAnyState("Start", startState);
        creatureAnimator.AddAnyState("Die", dieState);

        return creatureAnimator;
    }
}
