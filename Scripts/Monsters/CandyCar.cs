using ArknightsMap.Scripts.Powers;
using ArknightsMap.Scripts.Utils;
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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class CandyCar : AbstractSankta
{
    
    protected override int BulletMax => 0;
    protected override int InitialBullet => 0;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 35, 30);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 35, 30);
    private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 20, 20);

    
    
    private int Block => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);
    
    private bool IsTimeIncrease = false;

    
    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private bool IsBurningVineInCombat() => CombatState.Enemies.Any(e => e.IsAlive && e.IsMonster && e.Monster is BurningVine);

    

    private string GetAttackSfx() => "Attack";

    
    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        
        MoveState attack01 = new MoveState(
            "ATTACK01",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.8f);
                await DamageCmd.Attack(Damage).FromMonster(this).WithNoAttackerAnim().WithHitFx(sfx: GetAttackSfx()).Execute(null);
                foreach(Creature c in targets)
                {
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NFireSmokePuffVfx.Create(c));
                }
                await CreatureCmd.Kill(Creature);
            },
            new DeathBlowIntent(() => Damage)

        );
        

        

        list.Add(attack01);
        return new MonsterMoveStateMachine(list, attack01);
    }

    

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        
        AnimState attackState = new AnimState("Attack");

        AnimState dieState = new AnimState("Die");
        AnimState skillState = new AnimState("Skill");

        attackState.NextState = dieState;
        
        
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Skill", skillState);
        creatureAnimator.AddAnyState("Die", dieState);
        
        return creatureAnimator;
    }
}
