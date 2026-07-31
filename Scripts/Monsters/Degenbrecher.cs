using ArknightsMap.Scripts.Powers;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class Degenbrecher : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 900, 799);
    public override int MaxInitialHp => MinInitialHp;
    // public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");
    private int BlockedVulNum => 4;
    private int UnblockedVulNum => 2;
    private int BasicDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 9, 8);
    private int AdmitRequest => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 24, 20);
    
    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<WatchingPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if ((dealer == Creature) && (target.IsPlayer) && CreaturePositions.IsBlock(dealer, target))
        {
            return 1.5M;
        }

        return 1M;
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState attack = new MoveState(
            "DOUBLE_HIT",
            async targets =>
            {
                await DamageCmd.Attack(BasicDamage).WithHitCount(2).FromMonster(this).Execute(null);
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
            },
            new MultiAttackIntent(BasicDamage, 2), new BuffIntent()
        );

        MoveState skill = new MoveState(
            "EXPOSE",
            async targets =>
            {
                List<Creature> blockedCreatures = targets.Where(c => CreaturePositions.IsBlock(Creature, c)).ToList();
                List<Creature> unblockedCreatures = targets.Where(c => !CreaturePositions.IsBlock(Creature, c)).ToList();
                await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), unblockedCreatures, UnblockedVulNum, Creature, null);
                await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), blockedCreatures, BlockedVulNum, Creature, null);
                // 阻挡的人额外2层易伤
            },
            new DebuffIntent()
        );

        MoveState watch = new MoveState(
            "WATCH",
            targets => { return Task.CompletedTask; },
            new HiddenIntent()
        );

        MoveState join = new MoveState(
            "JOIN",
            async targets =>
            {
                await PowerCmd.Remove<WatchingPower>(Creature);
                foreach (Creature item in Creature.CombatState!.GetOpponentsOf(Creature))
                {
                    HiddenPower hiddenPower = (HiddenPower)ModelDb.Power<HiddenPower>().ToMutable();
                    hiddenPower.Target = item;
                    await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), hiddenPower, Creature, 1, Creature, null);
                }
                await PowerCmd.Apply<AdmitPower>(new ThrowingPlayerChoiceContext(), Creature, AdmitRequest, Creature, null);
                await PowerCmd.Apply<MomentumMurder>(new ThrowingPlayerChoiceContext(), Creature, (int)(Creature.MaxHp * 0.75), Creature, null);
            },
            new BuffIntent()
        );

        ConditionalBranchState condition = new ConditionalBranchState("CONDITION");
        condition.AddState(skill, () => Creature.GetPower<MomentumMurder>()?.TurnLeft >= 1);
        condition.AddState(attack, () => true);

        attack.FollowUpState = condition;
        watch.FollowUpState = join;
        join.FollowUpState = attack;
        skill.FollowUpState = condition;

        list.Add(condition);
        list.Add(attack);
        list.Add(skill);
        list.Add(watch);
        list.Add(join);

        return new MonsterMoveStateMachine(list, watch);
    }
}
