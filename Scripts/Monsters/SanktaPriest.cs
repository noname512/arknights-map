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
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class SanktaPriest : AbstractSankta
{

    protected override int BulletMax => 2;
    protected override int InitialBullet => 2;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 98, 96);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 104, 102);
    private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 7, 5);
    
    private int Block => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 15, 10);
    
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
               await DamageCmd.Attack(Damage).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
               await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
            },
            [new SingleAttackIntent(Damage), new DebuffIntent()]

        );
        MoveState skill_block = new MoveState(
            "SKILL_BLOCK",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill", 0.8f);
                foreach (var monster in CombatState.Enemies.Where(m => m.IsAlive))
                {
                    await UseBullet(1);
                    
                    await CreatureCmd.GainBlock(monster, Block, ValueProp.Move, null);
                    if (monster.Monster is AbstractSankta s)
                    {
                        await s.AddBullet(2);
                    }
                }
            },
            new BuffIntent()
        );

        MoveState skill_strength = new MoveState(
            "SKILL_STRENGTH",
            async targets =>
            {
                await UseBullet(1);
                await CreatureCmd.TriggerAnim(Creature, "Skill", 0.8f);
                foreach (var monster in CombatState.Enemies.Where(m => m.IsAlive))
                {
                    
                    await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), monster, 2, monster, null);
                    
                }
            },
            new BuffIntent()
        );

        
        skill_strength.FollowUpState = skill_block;
        skill_block.FollowUpState = attack;
        attack.FollowUpState = skill_strength;

        list.Add(attack);
        list.Add(skill_block);
        list.Add(skill_strength);
    

        return new MonsterMoveStateMachine(list, skill_strength);
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
