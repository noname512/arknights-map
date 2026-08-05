using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
<<<<<<< Updated upstream
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
=======
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
>>>>>>> Stashed changes
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class SanktaClaw : AbstractSankta
{
    protected override int BulletMax => 0;
    protected override int InitialBullet => 0;
<<<<<<< Updated upstream
=======
    
>>>>>>> Stashed changes

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 80, 80);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 90, 90);
    private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 16, 16);

    private int DamageBlock => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);

    private int Block => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);

    private int Damage_Skill => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 8, 8);

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<SanktaCreaturePower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    public int MoveInt = 0;

<<<<<<< Updated upstream
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private string GetAttackSfx() => "Attack";

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();

=======
    

    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");


    private string GetAttackSfx() => "Attack";

    

    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        
>>>>>>> Stashed changes
        MoveState attack = new MoveState(
            "ATTACK",
            async targets =>
            {
<<<<<<< Updated upstream
                await DamageCmd.Attack(Damage).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
            },
=======
                
                await DamageCmd.Attack(Damage).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                
                
            }, 
            
>>>>>>> Stashed changes
            new SingleAttackIntent(Damage)
        );

        MoveState attack_defend = new MoveState(
            "ATTACK_DEFEND",
            async targets =>
            {
<<<<<<< Updated upstream
                await DamageCmd.Attack(DamageBlock).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                await CreatureCmd.GainBlock(Creature, Block, ValueProp.Unpowered, null);
            },
=======
                
                await DamageCmd.Attack(DamageBlock).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                await CreatureCmd.GainBlock(Creature, Block, ValueProp.Unpowered, null);
                
                
            }, 
            
>>>>>>> Stashed changes
            [new SingleAttackIntent(DamageBlock), new DefendIntent()]
        );

        MoveState attack_debuff = new MoveState(
            "DEBUFF",
            async targets =>
            {
                await DamageCmd.Attack(Damage_Skill).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                foreach (Creature c in targets)
                {
                    await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), c, 2, c, null);
                }
<<<<<<< Updated upstream
            },
            [new SingleAttackIntent(Damage_Skill), new DebuffIntent()]
        );
=======
                
                
            }, 
            
            [new SingleAttackIntent(Damage_Skill), new DebuffIntent()]
        );
        

        

        
        
>>>>>>> Stashed changes

        attack.FollowUpState = attack_defend;
        attack_defend.FollowUpState = attack_debuff;
        attack_debuff.FollowUpState = attack;

        list.Add(attack);
        list.Add(attack_defend);
        list.Add(attack_debuff);
        return new MonsterMoveStateMachine(list, attack);
    }

    public override Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        var power = Creature.GetPower<PlunderPower>();
        if (side == CombatSide.Enemy && power != null && power.DynamicVars["HitTime"].BaseValue == 3)
        {
            foreach (Creature c in CombatState.PlayerCreatures)
<<<<<<< Updated upstream
            {
                PowerCmd.Apply<LoseEnergyNextTurnPower>(new ThrowingPlayerChoiceContext(), c, 2, c, null);
            }
            power.UpdateHitTime(0);
=======
                {
                    PowerCmd.Apply<LoseEnergyNextTurnPower>(new ThrowingPlayerChoiceContext(), c, 2, c, null);
                }
            power.DynamicVars["HitTime"].BaseValue = 0;
            power.InvokeDisplayAmountChanged();
>>>>>>> Stashed changes
        }
        return base.AfterSideTurnEndLate(choiceContext, side, participants);
    }

<<<<<<< Updated upstream
=======
    

>>>>>>> Stashed changes
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState startState = new AnimState("Start");
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState skillState = new AnimState("Skill");
<<<<<<< Updated upstream

        AnimState dieState = new AnimState("Die");

        attackState.NextState = idleState;
        skillState.NextState = idleState;

        startState.NextState = idleState;

=======
        

        AnimState dieState = new AnimState("Die");
        


        attackState.NextState = idleState;
        skillState.NextState = idleState;
        
        startState.NextState = idleState;
        
>>>>>>> Stashed changes
        CreatureAnimator creatureAnimator = new CreatureAnimator(startState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Skill", skillState);
        creatureAnimator.AddAnyState("Start", startState);
        creatureAnimator.AddAnyState("Die", dieState);

<<<<<<< Updated upstream
        return creatureAnimator;
    }
}
=======
        
        return creatureAnimator;
    }
}
>>>>>>> Stashed changes
