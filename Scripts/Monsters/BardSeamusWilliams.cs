using ArknightsMap.Scripts.Utils;
using Godot;
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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class BardSeamusWilliams : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 555, 540);
    public override int MaxInitialHp => MinInitialHp;

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");
    private int NormalAtkDmg => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 22, 20);
    private int BlinkDmg => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 32, 29);
    private int BlinkDmg2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 74, 70);
    private int ChargedDmg => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 120, 109);
    private int facing = 0;

    public override async Task AfterAddedToRoom()
    {
        // TODO: 被打断提示power
        facing = -1;
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack = new MoveState(
            "ATTACK",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0f);
                await Cmd.Wait(0.87f);
                await DamageCmd.Attack(NormalAtkDmg).FromMonster(this).WithNoAttackerAnim().Execute(null);
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 2, Creature, null);
            },
            new SingleAttackIntent(NormalAtkDmg),
            new BuffIntent()
        );
        bool HitWall()
        {
            if (facing == 0)
            {
                return false;
            }
            Creature p = CombatState.PlayerCreatures.FirstOrDefault()!;
            int CurPos = CreaturePositions.PositionOfCreature(Creature);
            int PlayerPos = CreaturePositions.PositionOfCreature(p);
            return (CurPos == 2 && PlayerPos == 1) || (CurPos == 8 && PlayerPos == 9);
        }
        MoveState blink = new MoveState(
            "BLINK",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill_Begin", 0.5f);
                if (!HitWall())
                {
                    await DamageCmd.Attack(BlinkDmg).FromMonster(this).WithNoAttackerAnim().Execute(null);
                    int CurPos = CreaturePositions.PositionOfCreature(Creature);
                    CreaturePositions.MoveTo(Creature, CurPos + facing * 2);
                    await CreatureCmd.TriggerAnim(Creature, "Skill_End", 0f);
                    await Cmd.Wait(1.5f);
                    facing = -facing;
                    Creature.GetCreatureNode()!.Body.Scale *= new Vector2(-1f, 1f);
                    List<Creature> list = new List<Creature>();
                    foreach (Creature c in CombatState.PlayerCreatures)
                    {
                        list.Add(c);
                        foreach (Creature p in c.Pets)
                        {
                            list.Add(p);
                        }
                    }
                    IEnumerable<Node2D> enumerable = list.Select((Creature c) => NCombatRoom.Instance?.GetCreatureNode(c)?.Body)!;
                    foreach (Node2D node in enumerable)
                        if (node != null && node.Scale.X * facing < 0)
                        {
                            node.Scale *= new Vector2(-1f, 1f);
                        }
                }
                else
                {
                    await DamageCmd.Attack(BlinkDmg2).FromMonster(this).WithNoAttackerAnim().Execute(null);
                }
            },
            new SingleAttackIntent(HitWall() ? BlinkDmg2 : BlinkDmg)
        );
        MoveState charging = new MoveState("CHARGING", async targets => { }, new UnknownIntent());
        MoveState chargedAttack = new MoveState(
            "CHARGED_ATTACK",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0f);
                await Cmd.Wait(0.87f);
                await DamageCmd.Attack(ChargedDmg).FromMonster(this).WithNoAttackerAnim().Execute(null);
            },
            new SingleAttackIntent(ChargedDmg)
        );

        RandomBranchState randomBranchState = new RandomBranchState("RANDOM");
        randomBranchState.AddBranch(attack, MoveRepeatType.CanRepeatForever, 1);
        randomBranchState.AddBranch(blink, MoveRepeatType.CanRepeatForever, 1);
        randomBranchState.AddBranch(charging, MoveRepeatType.CanRepeatForever, 1);
        RandomBranchState randomBranchState2 = new RandomBranchState("RANDOM2");
        randomBranchState2.AddBranch(attack, MoveRepeatType.CanRepeatForever, 1);
        randomBranchState2.AddBranch(blink, MoveRepeatType.CanRepeatForever, 1);
        attack.FollowUpState = randomBranchState;
        blink.FollowUpState = randomBranchState;
        charging.FollowUpState = chargedAttack;
        chargedAttack.FollowUpState = randomBranchState;

        list.Add(attack);
        list.Add(blink);
        list.Add(charging);
        list.Add(chargedAttack);
        list.Add(randomBranchState);
        list.Add(randomBranchState2);

        return new MonsterMoveStateMachine(list, blink);
    }

    public override Task OnWindBlow()
    {
        if (NextMove.Id == "CHARGING" || NextMove.Id == "CHARGED_ATTACK")
        {
            if (Rng.NextFloat() < 0.3)
            {
                CreatureCmd.Stun(Creature, "RANDOM2");
            }
            else
            {
                CreatureCmd.Stun(Creature, "CHARGING");
            }
        }
        return Task.CompletedTask;
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState skillBeginState = new AnimState("Skill_Begin");
        AnimState skillLoopState = new AnimState("Skill_Loop", isLooping: true);
        AnimState skillEndState = new AnimState("Skill_End");
        AnimState dieState = new AnimState("Die");

        attackState.NextState = idleState;
        skillBeginState.NextState = skillLoopState;
        skillEndState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Skill_Begin", skillBeginState);
        creatureAnimator.AddAnyState("Skill_End", skillEndState);
        creatureAnimator.AddAnyState("Die", dieState);

        return creatureAnimator;
    }
}
