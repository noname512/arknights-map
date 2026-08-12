using ArknightsMap.Scripts.Cards;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class OpCar : AbstractSankta
{
    protected override int BulletMax => 0;
    protected override int InitialBullet => 0;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 200, 200);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 200, 200);
    private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 5, 5);
    public int Time = 1;

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
                await UseBullet(1);
                await DamageCmd.Attack(Damage).FromMonster(this).WithAttackerAnim("Attack", 0.8f).WithHitFx(sfx: GetAttackSfx()).Execute(null);
                foreach (Creature c in targets)
                {
                    await CardPileCmd.AddToCombatAndPreview<Milk>(c, PileType.Draw, 1, null, CardPilePosition.Random);
                }
            },
            [new SingleAttackIntent(Damage), new StatusIntent(1)]
        );

        attack.FollowUpState = attack;

        list.Add(attack);

        return new MonsterMoveStateMachine(list, attack);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Start");
        AnimState dieState = new AnimState("Die");

        attackState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Start", attackState);
        creatureAnimator.AddAnyState("Die", dieState);
        attackState.NextState = idleState;

        return creatureAnimator;
    }

    public override Task BeforeDeath(Creature creature)
    {
        return base.BeforeDeath(creature);
    }
}
