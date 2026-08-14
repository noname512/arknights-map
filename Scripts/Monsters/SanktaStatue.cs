using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class SanktaStatue : AbstractSankta
{
    protected override int BulletMax => 0;
    protected override int InitialBullet => 0;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 35, 35);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 40, 40);
    private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 5);

    

    private int Block => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);

    

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<SanktaCreaturePower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    public int MoveInt = 0;

    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private string GetAttackSfx() => "Attack";

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();

        

        MoveState attack_debuff = new MoveState(
            "DEBUFF",
            async targets =>
            {
                await DamageCmd.Attack(Damage).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                foreach (Creature c in targets)
                {
                    await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), c, 2, c, null);
                }
            },
            [new SingleAttackIntent(Damage), new DebuffIntent()]
        );

        MoveState defend = new MoveState(
            "DEFEND",
            async targets =>
            {
                foreach (Creature c in CombatState.GetTeammatesOf(Creature))
                {
                    await CreatureCmd.GainBlock(c, Block, ValueProp.Unpowered, null);
                }
            },
            [new DefendIntent()]
        );

        
        attack_debuff.FollowUpState = defend;
        defend.FollowUpState = attack_debuff;

        
        list.Add(defend);
        list.Add(attack_debuff);
        return new MonsterMoveStateMachine(list, defend);
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
