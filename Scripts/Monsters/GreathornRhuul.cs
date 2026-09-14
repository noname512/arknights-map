using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class GreathornRhuul : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 195, 177);
    public override int MaxInitialHp => MinInitialHp;

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");
    private int Dmg1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 24, 22);
    private int Dmg2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 33, 30);
    private MapPoint? leftRoom;

    public override async Task AfterAddedToRoom()
    {
        //TODO: info power
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack1 = new MoveState(
            "ATTACK1",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Move", 0);
                await CreaturePositions.Walk(Creature, -1);
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0);
                await DamageCmd.Attack(Dmg1).FromMonster(this).Execute(null);
            },
            new MoveIntent(),
            new SingleAttackIntent(Dmg1)
        );
        MoveState attack2 = new MoveState(
            "ATTACK2",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Move", 0);
                await CreaturePositions.Walk(Creature, -1);
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0);
                await DamageCmd.Attack(Dmg2).FromMonster(this).Execute(null);
            },
            new MoveIntent(),
            new SingleAttackIntent(Dmg2)
        );
        MoveState charging = new MoveState(
            "CHARGNING",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill_Begin", 0);
            },
            new UnknownIntent() // TODO: 或许需要一个专门的蓄力intent
        );
        MoveState gore_n_left = new MoveState(
            "GORE_N_LEFT",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill_End", 0);
                await DamageCmd.Attack(Dmg2).FromMonster(this).Execute(null);
                if (leftRoom != null)
                {
                    await RunManager.Instance.EnterMapCoord(leftRoom.coord);
                }
            },
            new SingleAttackIntent(Dmg2)
        );
        MoveState gore_left = new MoveState(
            "GORE_LEFT",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill_End", 0);
                await DamageCmd.Attack(Dmg2).FromMonster(this).Execute(null);
                await PowerCmd.Apply<RingingPower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
            },
            new SingleAttackIntent(Dmg2),
            new DebuffIntent()
        );
        MoveState attack3 = new MoveState(
            "ATTACK3",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0);
                await DamageCmd.Attack(Dmg2).FromMonster(this).Execute(null);
            },
            new SingleAttackIntent(Dmg2)
        );

        attack1.FollowUpState = attack2;
        attack2.FollowUpState = charging;

        leftRoom = GetLeftRoom();
        ConditionalBranchState conditionalBranchState = new ConditionalBranchState("GORE");
        conditionalBranchState.AddState(gore_left, () => leftRoom == null);
        conditionalBranchState.AddState(gore_n_left, () => true);

        charging.FollowUpState = conditionalBranchState;
        gore_left.FollowUpState = attack3;
        attack3.FollowUpState = attack3;

        list.Add(attack1);
        list.Add(attack2);
        list.Add(charging);
        list.Add(gore_left);
        list.Add(gore_n_left);
        list.Add(conditionalBranchState);
        list.Add(attack3);

        return new MonsterMoveStateMachine(list, attack1);
    }

    private MapPoint? GetLeftRoom()
    {
        MapPoint currentMapPoint = CombatState.RunState.CurrentMapPoint!;
        MapPoint? previous = null;
        IEnumerable<MapPoint> points = CombatState.RunState.Map.GetPointsInRow(currentMapPoint.coord.row);
        foreach (MapPoint point in points)
        {
            if (point == currentMapPoint)
                break;
            previous = point;
        }
        return previous;
    }

    public override async void OnWindBlow()
    {
        if (NextMove.Intents.Any(i => i is UnknownIntent))
        {
            await CreatureCmd.TriggerAnim(Creature, "Skill_Break", 0);
            await CreatureCmd.Stun(Creature);
        }
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState moveState = new AnimState("Move");
        AnimState dieState = new AnimState("Die");
        AnimState skillBeginState = new AnimState("Skill_Begin");
        AnimState skillIdleState = new AnimState("Skill_Idle");
        AnimState skillEndState = new AnimState("Skill_End");
        AnimState skillBreakState = new AnimState("Skill_Break");
        AnimState stunEndState = new AnimState("Stun_End");

        attackState.NextState = idleState;
        skillBeginState.NextState = skillIdleState;
        skillEndState.NextState = idleState;
        moveState.NextState = idleState;
        skillBreakState.NextState = stunEndState;
        stunEndState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Skill_Begin", skillBeginState);
        creatureAnimator.AddAnyState("Skill_Break", skillBreakState);
        creatureAnimator.AddAnyState("Skill_End", skillEndState);
        creatureAnimator.AddAnyState("Die", dieState);

        return creatureAnimator;
    }
}
