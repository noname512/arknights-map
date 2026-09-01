using ArknightsMap.Scripts.Powers;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class OpCar : AbstractSankta
{
    protected override int BulletMax => 0;
    protected override int InitialBullet => 0;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 60, 60);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 60, 60);
    private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 3, 3);

    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    public NCreature CarPosition
    {
        get
        {
            var instance = NCombatRoom.Instance ?? throw new InvalidOperationException("Combat room instance is not available.");
            return instance.GetCreatureNode(Creature) ?? throw new InvalidOperationException("Creature node is not available.");
        }
    }

    public NCreature PlayerPosition
    {
        get
        {
            var instance = NCombatRoom.Instance ?? throw new InvalidOperationException("Combat room instance is not available.");
            return instance.GetCreatureNode(CombatState.GetOpponentsOf(Creature)[0]) ?? throw new InvalidOperationException("Creature node is not available.");
        }
    }

    public bool OnRight => PlayerPosition.Position.X < CarPosition.Position.X;

    public bool OnOtherSide()
    {
        var combat = Creature.CombatState;
        foreach (Creature c in combat!.GetTeammatesOf(Creature))
        {
            if (c.Monster is OpForGun gun && OnRight != gun.OnRight)
                return true;
        }
        return false;
    }

    public async Task UpdatePosition()
    {
        if (OnRight)
        {
            CarPosition.Visuals.Scale = new Vector2(1, 1);
        }
        else
        {
            CarPosition.Visuals.Scale = new Vector2(-1, 1);
        }
    }

    private string GetAttackSfx() => "Attack";

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<OpCarPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
        await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
        await PowerCmd.Apply<ShieldPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
        if (OnRight)
        {
            await PowerCmd.Apply<BackAttackRightPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
        }
        else
        {
            await UpdatePosition();
            await PowerCmd.Apply<BackAttackLeftPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();

        MoveState attack = new MoveState(
            "ATTACK",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.8f);
                await DamageCmd.Attack(Damage).FromMonster(this).WithHitFx(sfx: GetAttackSfx()).Execute(null);
            },
            [new SingleAttackIntent(Damage), new DebuffIntent()]
        );

        MoveState defend = new MoveState(
            "DEFEND",
            async targets =>
            {
                foreach (Creature c in CombatState.GetTeammatesOf(Creature))
                {
                    await CreatureCmd.GainBlock(c, 5, ValueProp.Unpowered, null);
                }
            },
            [new DefendIntent()]
        );

        MoveState attack_defend = new MoveState(
            "ATTACK_DEFEND",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.8f);
                await DamageCmd.Attack(12).FromMonster(this).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                await CreatureCmd.GainBlock(Creature, 8, ValueProp.Unpowered, null);
                foreach (Creature c in CombatState.GetOpponentsOf(Creature))
                {
                    if (c.Monster is not Osty)
                    {
                        await PowerCmd.Apply<CorrosionDamagePower>(new ThrowingPlayerChoiceContext(), c, 1m, Creature, null);
                    }
                }
            },
            [new SingleAttackIntent(12), new DefendIntent(), new DebuffIntent()]
        );

        RandomBranchState StartBranch = new RandomBranchState("RANDOM_BRANCH");

        ConditionalBranchState attackBranch = new ConditionalBranchState("ATTACK_BRANCH");

        ConditionalBranchState skillBranch = new ConditionalBranchState("SKILL_BRANCH");

        // attack 之后的分支：如果异侧，30% 概率用 attack_defend，否则 defend
        attackBranch.AddState(attack_defend, () => OnOtherSide() && Creature!.CombatState!.RunState.Rng.CombatTargets.NextFloat(0, 1) < 0.3f);
        attackBranch.AddState(defend, () => true); // 兜底

        // defend 之后的分支：如果异侧，30% 概率用 attack_defend，否则 attack
        skillBranch.AddState(attack_defend, () => OnOtherSide() && Creature!.CombatState!.RunState.Rng.CombatTargets.NextFloat(0, 1) < 0.3f);
        skillBranch.AddState(attack, () => true); // 兜底

        RandomBranchState attack_defendBranch = new RandomBranchState("ATTACK_DEFEND_BRANCH");

        attack_defendBranch.AddBranch(attack, MoveRepeatType.CanRepeatForever, 0.5f); // 兜底
        attack_defendBranch.AddBranch(defend, MoveRepeatType.CanRepeatForever, 0.5f); // 兜底

        StartBranch.AddBranch(attack, 10);
        StartBranch.AddBranch(defend, 10);
        attack.FollowUpState = attackBranch;
        defend.FollowUpState = skillBranch;
        attack_defend.FollowUpState = attackBranch;

        list.Add(attack);
        list.Add(defend);
        list.Add(attack_defend);
        list.Add(skillBranch);
        list.Add(attackBranch);
        list.Add(StartBranch);

        return new MonsterMoveStateMachine(list, StartBranch);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        var Gun = participants.FirstOrDefault(c => c.Monster is OpForGun);

        if (side == CombatSide.Enemy && Gun != null && Gun.Monster is OpForGun gun && gun.OnRight != OnRight)
        {
            await CreatureCmd.TriggerAnim(Creature, "Skill_a", 0.8f);
            await Cmd.Wait(1.0f);
            foreach (Creature c in CombatState.GetOpponentsOf(Creature))
            {
                if (c.Monster is not Osty)
                {
                    await PowerCmd.Apply<CorrosionDamagePower>(new ThrowingPlayerChoiceContext(), c, 1m, Creature, null);
                }
            }
        }
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Start");
        AnimState dieState = new AnimState("Die");
        AnimState skillState = new AnimState("Skill_a");

        attackState.NextState = idleState;
        skillState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Start", attackState);
        creatureAnimator.AddAnyState("Die", dieState);
        creatureAnimator.AddAnyState("Skill_a", attackState);

        attackState.NextState = idleState;

        return creatureAnimator;
    }

    public override Task BeforeDeath(Creature creature)
    {
        return base.BeforeDeath(creature);
    }
}
