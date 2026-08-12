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

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class OpForGun : AbstractSankta
{
    protected override int BulletMax => 15 + tolerance;
    protected override int InitialBullet => 0;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 800, 750);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 800, 750);

    private int run => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 20, 20);

    

    public static int tolerance = 0;

    

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
        await PowerCmd.Apply<BackAttackRightPower>(new ThrowingPlayerChoiceContext(), base.Creature, 1m, base.Creature, null);
    }

    

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();

        MoveState Run = new MoveState(
            "SUMMON",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill_1", 0.8f);
                await DamageCmd.Attack(run).FromMonster(this).Execute(null);
                Tween tween = tween = NCombatRoom.Instance.CreateTween().SetParallel().SetEase(Tween.EaseType.Out)
						.SetTrans(Tween.TransitionType.Cubic);
                if (OnRight)
                {
                    GunPosition.GlobalPosition = new Vector2(550.0f, GunPosition.GlobalPosition.Y);
                }
                else
                {
                    GunPosition.GlobalPosition = new Vector2(1450.0f, GunPosition.GlobalPosition.Y);
                }
                await UpdatePosition();
                
            },
            [new SingleAttackIntent(run)]
        );

        list.Add(Run);
        Run.FollowUpState = Run;
        return new MonsterMoveStateMachine(list, Run);
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
