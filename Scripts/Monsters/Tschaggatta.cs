using ArknightsMap.Scripts.Powers;
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
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class Tschaggatta : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 65, 54);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 65, 65);
    private int Damage1 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 10, 9);
    private int Damage2 => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 17, 15);
    private int FrailAmount => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 2, 1);
    private int Block1 => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 10, 8);

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState move1 = new MoveState(
            "ATTACK",
            async targets =>
                await DamageCmd
                    .Attack(Damage1)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.5f)
                    // .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                    .Execute(null),
            new SingleAttackIntent(Damage1)
        );
        MoveState specialMove1 = new MoveState(
            "SP_ATTACK",
            async targets =>
                await DamageCmd
                    .Attack(Damage1)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.5f)
                    // .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                    .Execute(null),
            new SingleAttackIntent(Damage1)
        );
        MoveState move2 = new MoveState(
            "DEBUFF",
            async targets => await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), targets, -1, Creature, null),
            new DebuffIntent()
        );
        MoveState move3 = new MoveState(
            "ATTACK_DEBUFF",
            async targets =>
            {
                await DamageCmd
                    .Attack(Damage2)
                    .FromMonster(this)
                    .WithAttackerAnim("Attack", 0.5f)
                    // .WithHitFx(sfx: $"event:/ArknightsMap/sfx/{GetType().Name}")
                    .Execute(null);
                await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, FrailAmount, Creature, null);
            },
            new SingleAttackIntent(Damage2),
            new DebuffIntent()
        );
        MoveState move4 = new MoveState(
            "BLOCK",
            async targets =>
            {
                foreach (Creature m in CombatState.Enemies)
                {
                    await CreatureCmd.GainBlock(m, Block1, ValueProp.Move, null);
                }
            },
            new DefendIntent()
        );
        MoveState specialMove2 = new MoveState("SUMMON", async targets => { }, new SummonIntent());

        ConditionalBranchState startState = new ConditionalBranchState("INIT");
        startState.AddState(move1, () => CombatState.ContainsMonster<Degenbrecher>() && Creature.SlotName == "6");
        startState.AddState(specialMove1, () => CombatState.ContainsMonster<Degenbrecher>() && Creature.SlotName == "7");
        startState.AddState(move1, () => Creature.SlotName == "5");
        startState.AddState(move2, () => Creature.SlotName == "6");
        startState.AddState(move3, () => Creature.SlotName == "7");
        startState.AddState(move4, () => Creature.SlotName == "8");
        startState.AddState(move1, () => true);

        ConditionalBranchState summonCondition = new ConditionalBranchState("SUMMON_COND");
        summonCondition.AddState(specialMove2, () => CombatState.Enemies.FirstOrDefault(e => e.Monster is Degenbrecher)?.HasPower<WatchingPower>() ?? false);
        summonCondition.AddState(move2, () => true);

        specialMove1.FollowUpState = summonCondition;
        specialMove2.FollowUpState = move2;
        move1.FollowUpState = move2;
        move2.FollowUpState = move3;
        move3.FollowUpState = move4;
        move4.FollowUpState = move1;
        list.Add(specialMove1);
        list.Add(specialMove2);
        list.Add(summonCondition);
        list.Add(move1);
        list.Add(move2);
        list.Add(move3);
        list.Add(move4);
        list.Add(startState);

        return new MonsterMoveStateMachine(list, startState);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState attackState = new AnimState("Attack");
        AnimState dieState = new AnimState("Die");
        attackState.NextState = idleState;
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Attack", attackState);
        creatureAnimator.AddAnyState("Dead", dieState);
        return creatureAnimator;
    }
}
