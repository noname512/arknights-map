using ArknightsMap.Scripts.Cards;
using ArknightsMap.Scripts.Powers;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Monsters;
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
public class OpCar : AbstractSankta
{
    protected override int BulletMax => 0;
    protected override int InitialBullet => 0;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 50, 50);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 50, 50);
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

        attack.FollowUpState = defend;
        defend.FollowUpState = attack;

        list.Add(attack);
        list.Add(defend);

        

        return new MonsterMoveStateMachine(list, attack);
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
