using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
public class Fortuna : AbstractSankta
{
    
    protected override int BulletMax => 6;
    protected override int InitialBullet => 6;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 216, 216);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 216, 216);
    private int Damage01 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 6, 6);

    private int Damage02 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 8);
    public int Time = 6;
    
    
    

    
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");


    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<BulletPower>(new ThrowingPlayerChoiceContext(), Creature, Bullet, Creature, null);
    }

    private string GetAttackSfx() => "Attack";

    public override Task AfterDamageReceivedLate(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == Creature && result.UnblockedDamage == 0 && Rng.NextDouble() < 0.7)
        {
            AddBullet(1);
        }
        return base.AfterDamageReceivedLate(choiceContext, target, result, props, dealer, cardSource);
    }

    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        
        MoveState attack01 = new MoveState(
            "Attack01",
            async targets =>
            {
                await UseBullet(6);
                await DamageCmd.Attack(Damage01).WithHitCount(Time).FromMonster(this).WithAttackerAnim("Attack01", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
            }, 
            
            new MultiAttackIntent(Damage01, () => Time)
        );
        MoveState attack02 = new MoveState(
            "Attack02",
            async targets =>
            {
                
                await DamageCmd.Attack(Damage02).FromMonster(this).WithAttackerAnim("Attack02", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
            }, 
            
            new SingleAttackIntent(Damage02)
        );
        MoveState skill = new MoveState(
            "Skill",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill_Begin", 0.8f);
                await AddBullet(2);
            },
            new BuffIntent()
        );

        ConditionalBranchState skillBranch = new ConditionalBranchState("SKILL_BRANCH");
skillBranch.AddState(attack01, () => Bullet >= BulletMax);
skillBranch.AddState(attack02, () => Bullet < BulletMax);

ConditionalBranchState attack02Branch = new ConditionalBranchState("ATTACK02_BRANCH");
attack02Branch.AddState(attack01, () => Bullet >= BulletMax);
attack02Branch.AddState(skill, () => Bullet < BulletMax);

attack01.FollowUpState = skill;
skill.FollowUpState = skillBranch;
attack02.FollowUpState = attack02Branch;

list.Add(attack01);
list.Add(attack02);
list.Add(skill);
list.Add(skillBranch);      // 分支状态也要加进 list
list.Add(attack02Branch);

return new MonsterMoveStateMachine(list, attack01);
        

}

    

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attack01State = new AnimState("Attack01");
        AnimState attack02State = new AnimState("Attack02");

        AnimState dieState = new AnimState("Die");
        AnimState skillbeginState = new AnimState("Skill_Begin");
        AnimState skillloopState = new AnimState("Skill_Loop");
        AnimState skillendState = new AnimState("Skill_End");


        attack01State.NextState = idleState;
        attack02State.NextState = idleState;
        
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack01", attack01State);
        creatureAnimator.AddAnyState("Attack02", attack02State);
        creatureAnimator.AddAnyState("Skill_Begin", skillbeginState);
        creatureAnimator.AddAnyState("Skill_Loop", skillloopState);
        creatureAnimator.AddAnyState("Skill_End", skillendState);
        creatureAnimator.AddAnyState("Die", dieState);
        skillbeginState.NextState = skillloopState;
        skillloopState.NextState = skillendState;
        skillendState.NextState = idleState;
        
        return creatureAnimator;
    }
}