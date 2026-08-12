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

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class SupersweetieSmiley : AbstractSankta
{
    protected override int BulletMax => 15 + tolerance;
    protected override int InitialBullet => 0;

    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 450, 450);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 450, 450);

    private int heavyAttack => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 20, 20);

    private int heavyAttackEnhanced => AscensionHelper.GetValueIfAscension(AscensionLevel.DoubleBoss, 30, 30);

    public static int tolerance = 0;

    public decimal Run => Creature?.GetPower<SSSPower>()?.DisplayAmount ?? 0;

    // 怪物场景
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), Creature, 2, Creature, null);
        await PowerCmd.Apply<SSSPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            var ssspower = Creature.GetPower<SSSPower>();
            if (ssspower != null && ssspower.DisplayAmount > 0)
            {
                await ssspower.UpdateTime(ssspower.DisplayAmount - 1);
            }
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();

        MoveState HeavyAttack = new MoveState(
            "HEAVY_ATTACK",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.8f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(heavyAttack).FromMonster(this).WithAttackerAnim("Attack", 0.8f).Execute(null);
                var ssspower = Creature.GetPower<SSSPower>();
                foreach (Creature c in targets)
                {
                    await CardPileCmd.AddToCombatAndPreview<Milk>(c, PileType.Draw, 2, null, CardPilePosition.Random);
                    await CardPileCmd.AddToCombatAndPreview<Milk>(c, PileType.Discard, 2, null, CardPilePosition.Random);
                }

                if (ssspower != null && ssspower.DynamicVars["Time"].BaseValue == 0)
                {
                    await CreatureCmd.Stun(Creature, "HEAVY_ATTACK");
                    tolerance += 5;
                }
            },
            [new SingleAttackIntent(heavyAttack), new StatusIntent(4)]
        );

        MoveState HeavyAttackEnhanced = new MoveState(
            "HEAVY_ATTACK_ENHANCED",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Attack", 0.8f);
                await Cmd.Wait(1.0f);
                await DamageCmd.Attack(heavyAttackEnhanced).FromMonster(this).WithAttackerAnim("Attack", 0.8f).Execute(null);
                foreach (Creature c in targets)
                {
                    await CardPileCmd.AddToCombatAndPreview<Milk>(c, PileType.Draw, 3, null, CardPilePosition.Random);
                    await CardPileCmd.AddToCombatAndPreview<Milk>(c, PileType.Discard, 3, null, CardPilePosition.Random);
                }

                var ssspower = Creature.GetPower<SSSPower>();
                if (ssspower != null && ssspower.DynamicVars["Time"].BaseValue == 0)
                {
                    await CreatureCmd.Stun(Creature, "HEAVY_ATTACK");
                    tolerance += 5;
                }
            },
            [new SingleAttackIntent(heavyAttackEnhanced), new StatusIntent(6)]
        );

        MoveState Splash = new MoveState(
            "SPLASH",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill_2", 0.8f);
                await Cmd.Wait(1.0f);
                await CreatureCmd.GainBlock(Creature, 15, ValueProp.Move, null);
                foreach (Creature c in targets)
                {
                    await CardPileCmd.AddToCombatAndPreview<Milk>(c, PileType.Hand, 2, null, CardPilePosition.Random);
                }
                await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, 2, Creature, null);

                var ssspower = Creature.GetPower<SSSPower>();
                if (ssspower != null && ssspower.DynamicVars["Time"].BaseValue == 0)
                {
                    await CreatureCmd.Stun(Creature, "HEAVY_ATTACK");
                    tolerance += 5;
                }
            },
            [new DebuffIntent(), new DefendIntent(), new StatusIntent(2)]
        );

        MoveState SplashEnhanced = new MoveState(
            "SPLASH_ENHANCED",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill_2", 0.8f);
                await Cmd.Wait(1.0f);
                await CreatureCmd.GainBlock(Creature, 20, ValueProp.Move, null);
                foreach (Creature c in targets)
                {
                    await CardPileCmd.AddToCombatAndPreview<Milk>(c, PileType.Hand, 3, null, CardPilePosition.Random);
                }
                await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), targets, 3, Creature, null);

                var ssspower = Creature.GetPower<SSSPower>();
                if (ssspower != null && ssspower.DynamicVars["Time"].BaseValue == 0)
                {
                    await CreatureCmd.Stun(Creature, "HEAVY_ATTACK");
                    tolerance += 5;
                }
            },
            [new DebuffIntent(), new DefendIntent(), new StatusIntent(3)]
        );

        MoveState Summon = new MoveState(
            "SUMMON",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill_1", 0.8f);
                await Cmd.Wait(1.0f);
                await CreatureCmd.Add<SupersweetieDeliveryDrone>(CombatState, CombatState.Encounter!.GetNextSlot(CombatState));
                await PowerCmd.Apply<MinionPower>(
                    new ThrowingPlayerChoiceContext(),
                    CombatState.Enemies.First(c => c.Monster is SupersweetieDeliveryDrone),
                    1m,
                    Creature,
                    null
                );

                var ssspower = Creature.GetPower<SSSPower>();
                if (ssspower != null && ssspower.DynamicVars["Time"].BaseValue == 0)
                {
                    await CreatureCmd.Stun(Creature, "HEAVY_ATTACK");
                    tolerance += 5;
                }
            },
            new SummonIntent()
        );

        MoveState SummonEnhanced = new MoveState(
            "SUMMON_ENHANCED",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Skill_1", 0.8f);
                await Cmd.Wait(1.0f);
                for (int i = 0; i < 2; i++)
                {
                    await CreatureCmd.Add<SupersweetieDeliveryDrone>(CombatState, CombatState.Encounter!.GetNextSlot(CombatState));
                }
                await PowerCmd.Apply<MinionPower>(
                    new ThrowingPlayerChoiceContext(),
                    CombatState.Enemies.First(c => c.Monster is SupersweetieDeliveryDrone),
                    1m,
                    Creature,
                    null
                );

                var ssspower = Creature.GetPower<SSSPower>();
                if (ssspower != null && ssspower.DynamicVars["Time"].BaseValue == 0)
                {
                    await CreatureCmd.Stun(Creature, "HEAVY_ATTACK");
                    tolerance += 5;
                }
            },
            new SummonIntent()
        );

        MoveState Wait = new MoveState(
            "WAIT",
            async targets =>
            {
                await CreatureCmd.TriggerAnim(Creature, "Stun_End", 0.8f);
                await PowerCmd.Apply<SSSPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
                await AddBullet(15 + tolerance);
                var ssspower = Creature.GetPower<SSSPower>();
                if (ssspower != null)
                {
                    await ssspower.UpdateTime(15 + tolerance);
                }
            },
            [new SleepIntent(), new AddBulletIntent()]
        );

        ConditionalBranchState HeavyBranch = new ConditionalBranchState("HEAVY_BRANCH");
        HeavyBranch.AddState(Splash, () => Run <= 5);
        HeavyBranch.AddState(SplashEnhanced, () => Run > 5);

        ConditionalBranchState SplashBranch = new ConditionalBranchState("SPLASH_BRANCH");
        SplashBranch.AddState(Summon, () => Run <= 5);
        SplashBranch.AddState(SummonEnhanced, () => Run > 5);

        ConditionalBranchState SummonBranch = new ConditionalBranchState("SUMMON_BRANCH");
        SummonBranch.AddState(HeavyAttack, () => Run <= 5);
        SummonBranch.AddState(HeavyAttackEnhanced, () => Run > 5);

        HeavyAttack.FollowUpState = HeavyBranch;
        HeavyAttackEnhanced.FollowUpState = HeavyBranch;
        Splash.FollowUpState = SplashBranch;
        SplashEnhanced.FollowUpState = SplashBranch;
        Summon.FollowUpState = SummonBranch;
        SummonEnhanced.FollowUpState = SummonBranch;
        Wait.FollowUpState = SummonBranch;

        list.Add(Wait);
        list.Add(HeavyAttack);
        list.Add(HeavyAttackEnhanced);
        list.Add(Splash);
        list.Add(SplashEnhanced);
        list.Add(Summon);
        list.Add(SummonEnhanced);
        list.Add(HeavyBranch);
        list.Add(SplashBranch);
        list.Add(SummonBranch);
        return new MonsterMoveStateMachine(list, Wait);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        AnimState skill1State = new AnimState("Skill_1");
        AnimState skill2State = new AnimState("Skill_2");
        AnimState startState = new AnimState("Stun_Loop", isLooping: true);
        AnimState startEndState = new AnimState("Stun_End");

        attackState.NextState = idleState;

        CreatureAnimator creatureAnimator = new CreatureAnimator(startState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Skill_1", skill1State);
        creatureAnimator.AddAnyState("Skill_2", skill2State);
        creatureAnimator.AddAnyState("Die", dieState);
        creatureAnimator.AddAnyState("Stun_Loop", startState);
        creatureAnimator.AddAnyState("Stun_End", startEndState);

        startEndState.NextState = idleState;
        attackState.NextState = idleState;
        skill1State.NextState = idleState;
        skill2State.NextState = idleState;

        return creatureAnimator;
    }
}
