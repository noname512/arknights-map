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
public class WastelandSkulker : AbstractSankta
{
    
    protected override int BulletMax => 0;
    protected override int InitialBullet => 0;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 30, 30);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 30, 30);
    private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 8);

    private int Block => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<SkulkerPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }
    

    public override decimal ModifyMaxEnergy(Player player, decimal amount)
	{
		if (Creature.IsDead)
		{
			return amount;
		}
		return amount - 1;
	}

    public bool HasOtherMonsterInCombat(){
        
        if (CombatState.HittableEnemies.Any(e => e.IsAlive && e.IsMonster && !(e.Monster is WastelandSkulker)))
        {
            return true;
        }
        return false;
    }

    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");


    private string GetAttackSfx() => "Attack";

    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        
        MoveState attack = new MoveState(
            "ATTACK",
            async targets =>
            {
                await DamageCmd.Attack(Damage).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
            }, 
            
            new SingleAttackIntent(Damage)
        );
        

        MoveState block = new MoveState(
            "BLOCK",
            async targets =>
            {
                await CreatureCmd.GainBlock(Creature, (decimal)Block, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, null);
            },
            new DefendIntent()
        );

        ConditionalBranchState attackBranch = new ConditionalBranchState("ATTACK_BRANCH");
        attackBranch.AddState(attack, () => !HasOtherMonsterInCombat());
        attackBranch.AddState(block, () => HasOtherMonsterInCombat());

        attack.FollowUpState = block;
        block.FollowUpState = attack;

        
        
        list.Add(attackBranch);
        list.Add(attack);
        list.Add(block);
        
    

        return new MonsterMoveStateMachine(list, attack);
    }

    

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState startState = new AnimState("Start");
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        

        AnimState dieState = new AnimState("Die");
        


        attackState.NextState = idleState;
        
        startState.NextState = idleState;
        
        CreatureAnimator creatureAnimator = new CreatureAnimator(startState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        
        creatureAnimator.AddAnyState("Start", startState);
        
        creatureAnimator.AddAnyState("Die", dieState);
        
        
        return creatureAnimator;
    }
}