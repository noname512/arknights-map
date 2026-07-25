using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
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
public class SanktaWitness : AbstractSankta
{
    
    protected override int BulletMax => 0;
    protected override int InitialBullet => 0;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 80, 85);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 90, 95);
    private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 12);

    private int MultiDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 6);

    public int Time = 1;
    
    
    private bool HasStatusInDraw(Player p) => p.PlayerCombatState.DrawPile.Cards.Any(c => c.Type == CardType.Status);

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
	{
        if (target == null)
    {
        return 1m;
    }
		if (target.Side != CombatSide.Player)
		{
			return 1m;
		}
		if (target.Player != null && HasStatusInDraw(target.Player))
		{
			return 1m;
		}
		if (!props.IsPoweredAttack())
		{
			return 1m;
		}
		if (dealer != Creature)
		{
			return 1m;
		}
		return 1.5m;
	}

    
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");
    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<FlutterPower>(new ThrowingPlayerChoiceContext(), Creature, 5, Creature, null);
    }

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

         MoveState multiattack = new MoveState(
            "MULTIATTACK",
            async targets =>
            {
                await DamageCmd.Attack(MultiDamage).FromMonster(this).WithHitCount(2).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
            }, 
            
            new SingleAttackIntent(MultiDamage)
        );

        MoveState skill = new MoveState(
            "SKILL",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.8f);
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 2, Creature, null);
            },
            new BuffIntent()
        );

        RandomBranchState startBranch = new RandomBranchState("START_BRANCH");
		startBranch.AddBranch(attack, MoveRepeatType.CannotRepeat);
		startBranch.AddBranch(multiattack, MoveRepeatType.CannotRepeat);
		startBranch.AddBranch(skill, MoveRepeatType.CannotRepeat);

        attack.FollowUpState = multiattack;
        multiattack.FollowUpState = skill;
        skill.FollowUpState = attack;
        list.Add(attack);
        list.Add(skill);
        list.Add(multiattack);
    

        return new MonsterMoveStateMachine(list, startBranch);
    }

    

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        

        attackState.NextState = idleState;
        
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Die", dieState);
        
        
        return creatureAnimator;
    }
}
