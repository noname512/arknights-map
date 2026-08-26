using Godot;
using ArknightsMap.Scripts.Cards;
using ArknightsMap.Scripts.Powers;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class OpForGun : AbstractSankta
{
    protected override int BulletMax => 15;
    protected override int InitialBullet => 0;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 500, 500);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 500, 500);

    private int run => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 20, 20);

    private int multi_attack => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 5, 5);

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
        await PowerCmd.Apply<SurroundedPower>(new ThrowingPlayerChoiceContext(), base.CombatState.GetOpponentsOf(base.Creature), 1m, base.Creature, null);
        
        await PowerCmd.Apply<OpForGunPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
        await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), base.Creature, 2m, base.Creature, null);
        await PowerCmd.Apply<ShieldPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
        await PowerCmd.Apply<BackAttackRightPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy && participants.Contains(Creature))
        {
            Attack_Time += 1;
        }
        return base.AfterSideTurnEnd(choiceContext, side, participants);
    }

    public override async Task AfterDamageReceivedLate(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource
    )
    {
        if (dealer == Creature && target.Monster is not Osty)
        {
            await PowerCmd.Apply<CorrosionDamagePower>(new ThrowingPlayerChoiceContext(), target, 1m, Creature, null);
        }
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
                    await PowerCmd.Apply<BackAttackLeftPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
                }
                else
                {
                    GunPosition.GlobalPosition = new Vector2(1450.0f, GunPosition.GlobalPosition.Y);
                    await CreatureCmd.Add<OpCar>(CombatState, "second_right");
                    await PowerCmd.Remove<BackAttackLeftPower>(Creature);
                    await PowerCmd.Apply<BackAttackRightPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
                }
                await UpdatePosition();
                Attack_Time = 1;
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
                
                
            },
            [new SingleAttackIntent(run), new SummonIntent(), new BuffIntent()]
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
                
            },
            [new DefendIntent()]
        );

        ConditionalBranchState RunBranch = new ConditionalBranchState(
            "RUN_BRANCH"
        );
        RunBranch.AddState(Prepare, () => ShouldRun());
        RunBranch.AddState(MultiHit, () => !ShouldRun());

        list.Add(Prepare);
        list.Add(Run);
        list.Add(MultiHit);
        list.Add(RunBranch);
        Prepare.FollowUpState = Run;
        Run.FollowUpState = RunBranch;
        MultiHit.FollowUpState = RunBranch;
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
