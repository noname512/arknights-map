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
public class BHCrossbowman : ModMonsterTemplate
{
    
    

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 85, 85);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 90, 90);
    private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);
    private int Block => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 15);

    private int Damage_Skill => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 18, 15);

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<CrossbowmanPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        MoveInt = CombatState.RunState.Rng.CombatTargets.NextInt(0,2);
        Creature.GetPower<CrossbowmanPower>().DynamicVars["HitTime"].BaseValue = MoveInt;
        Creature.GetPower<CrossbowmanPower>().InvokeDisplayAmountChanged();

    }

    public int MoveInt = 0;

    

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
                Creature.GetPower<CrossbowmanPower>().DynamicVars["HitTime"].BaseValue++;
                Creature.GetPower<CrossbowmanPower>().InvokeDisplayAmountChanged();
                
            }, 
            
            new SingleAttackIntent(Damage)
        );

        MoveState attack_defend = new MoveState(
            "ATTACK_DEFEND",
            async targets =>
            {
                
                await DamageCmd.Attack(Damage).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                await CreatureCmd.GainBlock(Creature, Block, ValueProp.Unpowered, null);
                Creature.GetPower<CrossbowmanPower>().DynamicVars["HitTime"].BaseValue++;
                Creature.GetPower<CrossbowmanPower>().InvokeDisplayAmountChanged();
                
            }, 
            
            [new SingleAttackIntent(Damage), new DefendIntent()]
        );

        MoveState debuff = new MoveState(
            "DEBUFF",
            async targets =>
            {
                
                foreach (Creature c in targets)
                {
                    await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), c, 2, c, null);
                }
                Creature.GetPower<CrossbowmanPower>().DynamicVars["HitTime"].BaseValue++;
                Creature.GetPower<CrossbowmanPower>().InvokeDisplayAmountChanged();
                
            }, 
            
            new DebuffIntent()
        );
        

        MoveState skill = new MoveState(
            "SKILL",
            async targets =>
            {
                var crossbowmanPower = Creature.GetPower<CrossbowmanPower>();   
                await DamageCmd.Attack(Damage_Skill).FromMonster(this).WithAttackerAnim("Skill", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                foreach (Creature c in targets)
                {
                    await PowerCmd.Apply<LoseEnergyNextTurnPower>(new ThrowingPlayerChoiceContext(), c, 1, c, null);
                }
                crossbowmanPower.DynamicVars["HitTime"].BaseValue = 0;
                crossbowmanPower.InvokeDisplayAmountChanged();
            },
            [new SingleAttackIntent(Damage_Skill), new DebuffIntent()]
        );

        
        ConditionalBranchState attackBranch = new ConditionalBranchState("ATTACK_BRANCH");
        attackBranch.AddState(attack_defend, () => Creature.GetPower<CrossbowmanPower>().DynamicVars["HitTime"].BaseValue < 3);
        attackBranch.AddState(skill, () => Creature.GetPower<CrossbowmanPower>().DynamicVars["HitTime"].BaseValue >= 3);

        ConditionalBranchState attackdefendBranch = new ConditionalBranchState("ATTACK_DEFEND_BRANCH");
        attackdefendBranch.AddState(debuff, () => Creature.GetPower<CrossbowmanPower>().DynamicVars["HitTime"].BaseValue < 3);
        attackdefendBranch.AddState(skill, () => Creature.GetPower<CrossbowmanPower>().DynamicVars["HitTime"].BaseValue >= 3);

        ConditionalBranchState debuffBranch = new ConditionalBranchState("DEBUFF_BRANCH");
        debuffBranch.AddState(attack, () => Creature.GetPower<CrossbowmanPower>().DynamicVars["HitTime"].BaseValue < 3);
        debuffBranch.AddState(skill, () => Creature.GetPower<CrossbowmanPower>().DynamicVars["HitTime"].BaseValue >= 3);


        attack.FollowUpState = attackBranch;
        attack_defend.FollowUpState = attackdefendBranch;
        debuff.FollowUpState = debuffBranch;
        skill.FollowUpState = attack;

        
        
        list.Add(attackBranch);
        list.Add(attackdefendBranch);
        list.Add(debuffBranch);
        list.Add(attack);
        list.Add(attack_defend);
        list.Add(debuff);
        list.Add(skill);
        return new MonsterMoveStateMachine(list, attack);
    }

    

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState startState = new AnimState("Start");
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState skillState = new AnimState("Skill");
        

        AnimState dieState = new AnimState("Die");
        


        attackState.NextState = idleState;
        skillState.NextState = idleState;
        
        startState.NextState = idleState;
        
        CreatureAnimator creatureAnimator = new CreatureAnimator(startState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Skill", skillState);
        creatureAnimator.AddAnyState("Start", startState);
        creatureAnimator.AddAnyState("Die", dieState);

        
        return creatureAnimator;
    }
}