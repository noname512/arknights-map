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
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class PathfinderWarrior : AbstractSankta
{
    protected override int BulletMax => 1;
    protected override int InitialBullet => 1;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 53, 48);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 57, 52);
    private int Damage01 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);

    private int Damage02 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 4, 4);

    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private string GetAttackSfx() => "Attack";

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();

        MoveState attack01 = new MoveState(
            "ATTACK01",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.8f);
                await DamageCmd.Attack(Damage01).FromMonster(this).WithNoAttackerAnim().WithHitFx(sfx: GetAttackSfx()).Execute(null);
            },
            [new SingleAttackIntent(Damage01)]
        );
        MoveState attack02 = new MoveState(
            "ATTACK02",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.8f);
                await DamageCmd.Attack(Damage02).FromMonster(this).WithHitCount(2).WithNoAttackerAnim().WithHitFx(sfx: GetAttackSfx()).Execute(null);
            },
            [new MultiAttackIntent(Damage02, 2)]
        );

        MoveState buff = new MoveState(
            "BUFF",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.8f);
                await Cmd.Wait(1.0f);
                foreach (Creature c in CombatState.HittableEnemies)
                {
                    await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), c, 1, Creature, null);
                }
                await UseBullet(1);
            },
            [new BuffIntent(), new UseBulletIntent()]
        );

        RandomBranchState startBranch = new RandomBranchState("START_BRANCH");
        startBranch.AddBranch(attack01, MoveRepeatType.CannotRepeat);
        startBranch.AddBranch(attack02, MoveRepeatType.CannotRepeat);
        startBranch.AddBranch(buff, MoveRepeatType.CannotRepeat);

        RandomBranchState attackBranch = new RandomBranchState("ATTACK_BRANCH");
        attackBranch.AddBranch(attack01, MoveRepeatType.CanRepeatForever);
        attackBranch.AddBranch(attack02, MoveRepeatType.CanRepeatForever);

        ConditionalBranchState buffBranch = new ConditionalBranchState("BUFF_BRANCH");
        buffBranch.AddState(buff, () => Bullet > 0);
        buffBranch.AddState(attackBranch, () => Bullet <= 0);

        list.Add(attack01);
        list.Add(attack02);
        list.Add(buff);
        list.Add(startBranch);
        list.Add(attackBranch);
        list.Add(buffBranch);

        attack01.FollowUpState = buffBranch;
        attack02.FollowUpState = buffBranch;
        buff.FollowUpState = attackBranch;

        return new MonsterMoveStateMachine(list, startBranch);
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
        creatureAnimator.AddAnyState("Die", dieState);

        return creatureAnimator;
    }
}
