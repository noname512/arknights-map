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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class PathfinderCannon : AbstractSankta
{
    
    protected override int BulletMax => 2;
    protected override int InitialBullet => 2;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 118, 116);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 124, 122);
    private int Damage01 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 20, 18);

    private int Damage02 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);
    
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
                await CreatureCmd.TriggerAnim(Creature, "Attack_B", 0.8f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(Damage01).FromMonster(this).WithNoAttackerAnim().WithHitFx(sfx: GetAttackSfx()).Execute(null);
                
                await UseBullet(1);
                foreach(Creature c in targets)
                {
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NFireSmokePuffVfx.Create(c));
                }
            },
            [new SingleAttackIntent(Damage01)]

        );
        MoveState attack02 = new MoveState(
            "ATTACK02",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack_A", 0.8f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(Damage02).FromMonster(this).WithNoAttackerAnim().WithHitFx(sfx: GetAttackSfx()).Execute(null);
                await CreatureCmd.GainBlock(Creature, Block, ValueProp.Move, null);
            },
            [new SingleAttackIntent(Damage02), new DefendIntent()]
        );

        ConditionalBranchState attackBranch = new ConditionalBranchState("ATTACK_BRANCH");
    attackBranch.AddState(attack01, () => Bullet > 0);
    attackBranch.AddState(attack02, () => Bullet <= 0);

        list.Add(attack01);
        list.Add(attack02);
        list.Add(attackBranch);

        attack01.FollowUpState = attackBranch;
        attack02.FollowUpState = attackBranch;
        return new MonsterMoveStateMachine(list, attack01);
    }

    

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attack01State = new AnimState("Attack_B");
        AnimState attack02State = new AnimState("Attack_A");

        AnimState dieState = new AnimState("Die");
        AnimState skillState = new AnimState("Skill");

        attack01State.NextState = idleState;
        attack02State.NextState = idleState;
        
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack_B", attack01State);
        creatureAnimator.AddAnyState("Attack_A", attack02State);
        creatureAnimator.AddAnyState("Skill", skillState);
        creatureAnimator.AddAnyState("Die", dieState);
        
        return creatureAnimator;
    }
}
