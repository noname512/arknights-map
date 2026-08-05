using ArknightsMap.Scripts.Powers;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
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

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 118, 116);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 124, 122);
    private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 20, 18);
    public int Time = 1;

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
                await UseBullet(1);
                await DamageCmd.Attack(Damage).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
            },
            [new SingleAttackIntent(Damage), new UseBulletIntent()]
        );
        MoveState skill = new MoveState(
            "SKILL",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill_Start", 0.8f);
                await PowerCmd.Apply<BulletPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
            },
            new AddBulletIntent()
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
