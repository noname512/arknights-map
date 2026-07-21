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
public class SanktaSniper : AbstractSankta
{
    
    protected override int BulletMax => 1;
    protected override int InitialBullet => 0;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 88, 86);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 94, 92);
    private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 12);
    public int Time = 1;
    
    
    private bool IsTimeIncrease = false;

    
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private bool IsBurningVineInCombat() => CombatState.Enemies.Any(e => e.IsAlive && e.IsMonster && e.Monster is BurningVine);

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<BulletPower>(new ThrowingPlayerChoiceContext(), Creature, Bullet, Creature, null);
    }

    private string GetAttackSfx() => "Attack";

    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        
        MoveState attack = new MoveState(
            "ATTACK",
            async targets =>
            {
                await UseBullet(1);
                await DamageCmd.Attack(Damage).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
            }, 
            
            new SingleAttackIntent(Damage)
        );
        MoveState skill = new MoveState(
            "SKILL",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill_Start", 0.8f);
                await PowerCmd.Apply<BulletPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
            },
            new BuffIntent()
        );

        ConditionalBranchState attackBranch = new ConditionalBranchState("ATTACK_BRANCH");
        attackBranch.AddState(attack, () => Bullet > 0);
        attackBranch.AddState(skill, () => Bullet <= 0);
        attack.FollowUpState = attackBranch;
        skill.FollowUpState = attackBranch;
        list.Add(attack);
        list.Add(skill);
        list.Add(attackBranch);
    

        return new MonsterMoveStateMachine(list, skill);
    }

    

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        AnimState skillstartState = new AnimState("Skill_Start");
        AnimState skillloopState = new AnimState("Skill_Loop");
        AnimState skillendState = new AnimState("Skill_End");


        attackState.NextState = idleState;
        
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Skill_Start", skillstartState);
        creatureAnimator.AddAnyState("Skill_Loop", skillloopState);
        creatureAnimator.AddAnyState("Skill_End", skillendState);
        creatureAnimator.AddAnyState("Die", dieState);
        skillstartState.NextState = skillloopState;
        skillloopState.NextState = skillendState;
        skillendState.NextState = idleState;
        
        return creatureAnimator;
    }
}
