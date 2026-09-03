using ArknightsMap.Scripts.Powers;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
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
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class PathfinderMage : AbstractSankta
{
    protected override int BulletMax => 1;
    protected override int InitialBullet => 1;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 58, 58);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 62, 62);
    private int Damage01 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 12);

    private int Damage02 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 10);

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
                await Cmd.Wait(1.0f);
                AttackCommand attack = await DamageCmd.Attack(Damage01).FromMonster(this).WithNoAttackerAnim().WithHitFx(sfx: GetAttackSfx()).Execute(null);
                await UseBullet(1);
                foreach (Creature c in targets)
                {
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NFireSmokePuffVfx.Create(c));
                    await PowerCmd.Apply<FlamingDamagePower>(
                        new ThrowingPlayerChoiceContext(),
                        c,
                        attack.Results.SelectMany(r => r).Sum(r => r.TotalDamage),
                        Creature,
                        null
                    );
                }
            },
            [new SingleAttackIntent(Damage01), new DebuffIntent(), new UseBulletIntent()]
        );
        MoveState attack02 = new MoveState(
            "ATTACK02",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.8f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(Damage02).FromMonster(this).WithNoAttackerAnim().WithHitFx(sfx: GetAttackSfx()).Execute(null);
            },
            [new SingleAttackIntent(Damage02)]
        );

        MoveState debuff = new MoveState(
            "DEBUFF",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.8f);
                await Cmd.Wait(1.0f);
                foreach (Creature c in targets)
                {
                    await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), c, 2, Creature, null);
                }
            },
            [new DebuffIntent()]
        );

        RandomBranchState startBranch = new RandomBranchState("START_BRANCH");
        startBranch.AddBranch(attack01, MoveRepeatType.CannotRepeat);
        startBranch.AddBranch(attack02, MoveRepeatType.CannotRepeat);
        startBranch.AddBranch(debuff, MoveRepeatType.CannotRepeat);

        ConditionalBranchState attackBranch = new ConditionalBranchState("ATTACK_BRANCH");
        attackBranch.AddState(attack01, () => Bullet > 0);
        attackBranch.AddState(debuff, () => Bullet <= 0);

        ConditionalBranchState debuffBranch = new ConditionalBranchState("DEBUFF_BRANCH");
        debuffBranch.AddState(attack01, () => Bullet > 0);
        debuffBranch.AddState(attack02, () => Bullet <= 0);

        list.Add(attack01);
        list.Add(attack02);
        list.Add(debuff);
        list.Add(startBranch);
        list.Add(attackBranch);
        list.Add(debuffBranch);

        attack01.FollowUpState = attack02;
        attack02.FollowUpState = attackBranch;
        debuff.FollowUpState = debuffBranch;
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
