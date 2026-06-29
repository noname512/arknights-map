using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class EndPoint : ModMonsterTemplate
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 200, 195);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 200, 195);
    private int Damage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 12, 11);
    private int TargetNum => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private int stage = 1;
    private Node2D? StartBody;

    public override bool ShouldAllowTargeting(Creature target)
    {
        return target != Creature || stage == 2;
    }

    public override bool IsHealthBarVisible => stage == 2;

    public override async Task AfterAddedToRoom()
    {
        StartBody = Creature.GetCreatureNode()!.Visuals.GetNode<Node2D>("%Visuals2");
        StartBody.Visible = true;
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState summon1 = new MoveState(
            "SUMMON_BURNING_VINE",
            async targets => await CreatureCmd.Add<BurningVine>(CombatState, "BurningVine"),
            new SummonIntent()
        );
        MoveState summon2 = new MoveState(
            "SUMMON_PATROLLING_FOLIAGE",
            async targets => await CreatureCmd.Add<PatrollingFoliage>(CombatState, "PatrollingFoliage"),
            new SummonIntent()
        );
        MoveState summon3 = new MoveState(
            "SUMMON_TREE_SHIELD",
            async targets => await CreatureCmd.Add<TreeShield>(CombatState, "TreeShield"),
            new SummonIntent()
        );
        MoveState summon4 = new MoveState(
            "SUMMON_ASH_CREATION",
            async targets => await CreatureCmd.Add<AshCreation>(CombatState, "AshCreation"),
            new SummonIntent()
        );
        MoveState show = new MoveState("SHOW", Show, new SummonIntent());
        MoveState stun = new MoveState("STUN", async targets => { }, new StunIntent());
        MoveState foxMove = new MoveState(
            "FOX_MOVE",
            async targets =>
            {
                await DamageCmd
                    .Attack(Damage)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.5f)
                    // .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                    .Execute(null);

                List<Creature> monsters = CombatState.Enemies.Where(e => e.IsAlive && e != Creature).ToList();
                for (int i = 0; i < TargetNum; i++)
                {
                    Creature? c = Rng.NextItem(monsters);
                    if (c != null)
                    {
                        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), c, 2, Creature, null);
                        monsters.Remove(c);
                    }
                }
            },
            new SingleAttackIntent(Damage),
            new BuffIntent()
        );

        ConditionalBranchState conditionalBranchState = new ConditionalBranchState("SHOW?");
        conditionalBranchState.AddState(summon4, () => RunManager.Instance.HasAscension(AscensionLevel.DeadlyEnemies));
        conditionalBranchState.AddState(show, () => !RunManager.Instance.HasAscension(AscensionLevel.DeadlyEnemies));
        summon1.FollowUpState = summon2;
        summon2.FollowUpState = summon3;
        summon3.FollowUpState = conditionalBranchState;
        summon4.FollowUpState = show;
        show.FollowUpState = foxMove;
        stun.FollowUpState = foxMove;
        foxMove.FollowUpState = foxMove;
        list.Add(summon1);
        list.Add(summon2);
        list.Add(summon3);
        list.Add(summon4);
        list.Add(show);
        list.Add(conditionalBranchState);
        list.Add(stun);
        list.Add(foxMove);

        return new MonsterMoveStateMachine(list, summon1);
    }

    public async Task Show(IReadOnlyList<Creature> targets)
    {
        stage = 2;
        StartBody!.Visible = false;
        Creature.GetCreatureNode()!.ToggleIsInteractable(true);
        await CreatureCmd.TriggerAnim(Creature, "Start", 0);
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
        if (target != Creature || props.HasFlag(ValueProp.Unpowered))
        {
            return;
        }
        await Show([]);
        SetMoveImmediate((MoveState)MoveStateMachine!.States["STUN"]);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        AnimState invisibleState = new AnimState("Invisible");
        AnimState startState = new AnimState("Start");
        attackState.NextState = idleState;
        startState.NextState = idleState;
        CreatureAnimator creatureAnimator = new CreatureAnimator(invisibleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Start", startState);
        creatureAnimator.AddAnyState("Dead", dieState);
        return creatureAnimator;
    }
}
