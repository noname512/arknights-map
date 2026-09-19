using ArknightsMap.Scripts.Powers;
using ArknightsMap.Scripts.Utils;
using Godot;
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
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class OpForGun : AbstractSankta
{
    protected override int BulletMax => 1;
    protected override int InitialBullet => 0;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 500, 500);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 500, 500);

    private int run => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 25, 25);

    private int multi_attack => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 6, 6);

    public int Attack_Time = 1;

    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    public NCreature GunPosition
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

    public bool OnRight => PlayerPosition.Position.X < GunPosition.Position.X;

    public async Task UpdatePosition()
    {
        if (OnRight)
        {
            GunPosition.Visuals.Scale = new Vector2(1.5f, 1.5f);
        }
        else
        {
            GunPosition.Visuals.Scale = new Vector2(-1.5f, 1.5f);
        }
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<SurroundedPower>(new ThrowingPlayerChoiceContext(), CombatState.GetOpponentsOf(Creature), 1m, Creature, null);

        await PowerCmd.Apply<OpForGunPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
        await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), Creature, 2m, Creature, null);
        await PowerCmd.Apply<ShieldPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
        await PowerCmd.Apply<BackAttackRightPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy && participants.Contains(Creature))
        {
            Attack_Time += 1;
        }
        return Task.CompletedTask;
    }

    public bool ShouldRun()
    {
        bool shouldRun = true;
        foreach (Creature c in CombatState.GetTeammatesOf(Creature))
        {
            if (c.Monster is OpCar car && car.OnRight != OnRight)
            {
                shouldRun = false;
                break;
            }
        }
        return shouldRun;
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();

        MoveState Run = new MoveState(
            "RUN",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill_1", 0.8f);
                await DamageCmd.Attack(run).FromMonster(this).Execute(null);
                if (OnRight)
                {
                    GunPosition.GlobalPosition = new Vector2(550.0f, GunPosition.GlobalPosition.Y);
                    await CreatureCmd.Add<OpCar>(CombatState, "second_left");
                    await PowerCmd.Remove<BackAttackRightPower>(Creature);
                    await PowerCmd.Apply<BackAttackLeftPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
                }
                else
                {
                    GunPosition.GlobalPosition = new Vector2(1450.0f, GunPosition.GlobalPosition.Y);
                    await CreatureCmd.Add<OpCar>(CombatState, "second_right");
                    await PowerCmd.Remove<BackAttackLeftPower>(Creature);
                    await PowerCmd.Apply<BackAttackRightPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
                }
                await UpdatePosition();
                Attack_Time = 1;
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
                await UseBullet(1);
            },
            [new SingleAttackIntent(run), new SummonIntent(), new BuffIntent(), new UseBulletIntent()]
        );

        MoveState MultiHit = new MoveState(
            "MULTI_HIT",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.8f);
                await DamageCmd.Attack(multi_attack).WithHitCount(Attack_Time).FromMonster(this).Execute(null);
            },
            [new MultiAttackIntent(multi_attack, () => Attack_Time)]
        );

        MoveState Prepare = new MoveState(
            "PREP",
            async targets =>
            {
                await CreatureCmd.GainBlock(Creature, 30, ValueProp.Unpowered, null);
                await AddBullet(1);
            },
            [new DefendIntent(), new AddBulletIntent()]
        );

        ConditionalBranchState PrepareBranch = new ConditionalBranchState("PREP_BRANCH");
        PrepareBranch.AddState(Prepare, () => Bullet <= 0 && ShouldRun());
        PrepareBranch.AddState(MultiHit, () => Bullet <= 0 && !ShouldRun());
        PrepareBranch.AddState(Run, () => Bullet > 0);

        

        list.Add(Prepare);
        list.Add(Run);
        list.Add(MultiHit);
        list.Add(PrepareBranch);
        Prepare.FollowUpState = PrepareBranch;
        Run.FollowUpState = PrepareBranch;
        MultiHit.FollowUpState = PrepareBranch;
        return new MonsterMoveStateMachine(list, MultiHit);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        AnimState RunState = new AnimState("Skill_1");
        AnimState skill2State = new AnimState("Skill_2");
        AnimState startState = new AnimState("Stun_Loop", isLooping: true);
        AnimState startEndState = new AnimState("Stun_End");

        attackState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Skill_1", RunState);
        creatureAnimator.AddAnyState("Skill_2", skill2State);
        creatureAnimator.AddAnyState("Die", dieState);
        creatureAnimator.AddAnyState("Stun_Loop", startState);
        creatureAnimator.AddAnyState("Stun_End", startEndState);

        startEndState.NextState = idleState;
        attackState.NextState = idleState;
        RunState.NextState = idleState;
        skill2State.NextState = idleState;

        return creatureAnimator;
    }
}
