using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class DublinnEvocator : AbstractWildsMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 73, 67);
    public override int MaxInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 81, 77);
    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn"
    );

    private int ExplodeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 25, 23);

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState summon = new MoveState("SUMMON", Summon, new SummonIntent());

        summon.FollowUpState = summon;

        list.Add(summon);

        return new MonsterMoveStateMachine(list, summon);
    }

    public async Task Summon(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.TriggerAnim(Creature, "Skill", 0);
        string position = CombatState.Encounter?.Slots.LastOrDefault<string>((s => CombatState.Enemies.All<Creature>((Func<Creature, bool>)(c => c.SlotName != s))), string.Empty);
        if (!string.IsNullOrEmpty(position))
        {
            await CreatureCmd.Add<Fireball>(CombatState, position);
        }
        else
        {
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), targets, new DamageVar(ExplodeDamage, ValueProp.Unpowered), Creature);
            await PowerCmd.Apply<FlamingDamagePower>(new ThrowingPlayerChoiceContext(), targets, ExplodeDamage, Creature, null);
        }
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idleState = new AnimState("Idle", isLooping: true);
        AnimState skillState = new AnimState("Skill");
        AnimState dieState = new AnimState("Die");
        skillState.NextState = idleState;
        CreatureAnimator creatureAnimator = new CreatureAnimator(idleState, controller);
        creatureAnimator.AddAnyState("Dead", dieState);
        creatureAnimator.AddAnyState("Skill", skillState);
        return creatureAnimator;
    }
}