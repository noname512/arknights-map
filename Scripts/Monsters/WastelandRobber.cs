using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class WastelandRobber : ModMonsterTemplate
{
    
    

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 30, 30);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 30, 30);
    private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);

    private int Block => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);
    private int DamageMulti => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 6);

    
    

    public override Task AfterDamageReceivedLate(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == Creature && result.UnblockedDamage > 0)
        {
            foreach (Creature c in CombatState.PlayerCreatures)
            {
                PowerCmd.Apply<LoseEnergyNextTurnPower>(new ThrowingPlayerChoiceContext(), c, 1, c, null);
            }
        }
        return base.AfterDamageReceivedLate(choiceContext, target, result, props, dealer, cardSource);
    }
    
    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<RobberPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }
    
    private bool IsTimeIncrease = false;

    
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private bool IsBurningVineInCombat() => CombatState.Enemies.Any(e => e.IsAlive && e.IsMonster && e.Monster is BurningVine);

    

    private string GetAttackSfx() => "Attack";

    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        
        MoveState attack = new MoveState(
            "ATTACK",
            async targets =>
            {
                await DamageCmd.Attack(Damage).FromMonster(this).WithAttackerAnim("Attack_2", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
            }, 
            
            new SingleAttackIntent(Damage)
        );
        MoveState multiAttack = new MoveState(
            "MULTI_ATTACK",
            async targets =>
            {
                await DamageCmd.Attack(DamageMulti).FromMonster(this).WithHitCount(2).WithAttackerAnim("Attack_1", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
            }, 
            
            new MultiAttackIntent(DamageMulti, 2)
        );

        MoveState block = new MoveState(
            "BLOCK",
            async targets =>
            {
                await CreatureCmd.GainBlock(Creature, (decimal)Block, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, null);
            },
            new DefendIntent()
        );

        
        attack.FollowUpState = block;
        block.FollowUpState = multiAttack;
        multiAttack.FollowUpState = attack;
        list.Add(attack);
        list.Add(multiAttack);
        list.Add(block);
        
    

        return new MonsterMoveStateMachine(list, attack);
    }

    

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState startState = new AnimState("Start");
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState1 = new AnimState("Attack_1");
        AnimState attackState2 = new AnimState("Attack_2");

        AnimState dieState = new AnimState("Die");
        


        attackState1.NextState = idleState;
        attackState2.NextState = idleState;
        startState.NextState = idleState;
        
        CreatureAnimator creatureAnimator = new CreatureAnimator(startState, controller);
        creatureAnimator.AddAnyState("Attack_1", attackState1);
        creatureAnimator.AddAnyState("Attack_2", attackState2);
        creatureAnimator.AddAnyState("Start", startState);
        
        creatureAnimator.AddAnyState("Die", dieState);
        
        
        return creatureAnimator;
    }
}