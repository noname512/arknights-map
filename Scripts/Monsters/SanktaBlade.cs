using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class SanktaBlade : AbstractSankta
{
    
    protected override int BulletMax => 3;
    protected override int InitialBullet => 3;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 88, 86);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 94, 92);
    private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 8);
    public int Time = 1;
    private int Strength => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 5);
    
    private bool IsTimeIncrease = false;

    private int AttackTime
	{
		get
		{
			return Time;
		}
		set
		{
			AssertMutable();
			Time = value;
		}
	}
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private bool IsBurningVineInCombat() => CombatState.Enemies.Any(e => e.IsAlive && e.IsMonster && e.Monster is BurningVine);

    

    private string GetAttackSfx() => "Attack";

    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        
        MoveState attack = new MoveState(
            "ATTACK",
            async targets => await DamageCmd.Attack(Damage).WithHitCount(AttackTime).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null), 
            
            new MultiAttackIntent(Damage, () => AttackTime)
        );
        MoveState skill = new MoveState(
            "SKILL",
            async targets =>
            {
                if (!IsTimeIncrease)
                {
                    await UseBullet(1);
                    await CreatureCmd.TriggerAnim(Creature, "Skill", 0.8f);
                    await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, Strength, Creature, null);
                    IsTimeIncrease = true;
                }
                else
                {
                    await UseBullet(1);
                    await CreatureCmd.TriggerAnim(Creature, "Skill", 0.8f);
                    AttackTime ++ ;
                    IsTimeIncrease = false;
                }
            },
            new BuffIntent()
        );

        ConditionalBranchState attackBranch = new ConditionalBranchState("ATTACK_BRANCH");
    attackBranch.AddState(skill, () => Bullet > 0);
    attackBranch.AddState(attack, () => Bullet <= 0);

    attack.FollowUpState = attackBranch;
        skill.FollowUpState = attack;
        list.Add(attack);
        list.Add(skill);
    

        return new MonsterMoveStateMachine(list, attack);
    }

    

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        AnimState skillState = new AnimState("Skill");

        attackState.NextState = idleState;
        
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Skill", skillState);
        creatureAnimator.AddAnyState("Die", dieState);
        
        return creatureAnimator;
    }
}
