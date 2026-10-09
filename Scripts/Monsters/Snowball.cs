using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class Snowball : AbstractSnowyMountainMonster
{
    public override int MinInitialHp => AscensionHelper.GetValueIfAscension(AscensionLevel.ToughEnemies, 120, 109);
    public override int MaxInitialHp => MinInitialHp;
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    private int ExplodeDamage => AscensionHelper.GetValueIfAscension(AscensionLevel.DeadlyEnemies, 25, 23);

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<TauntPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        await PowerCmd.Apply<RollingPower>(new ThrowingPlayerChoiceContext(), Creature, ExplodeDamage, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState state = new MoveState("STATE", async targets => { }, new UnknownIntent());

        state.FollowUpState = state;

        list.Add(state);

        return new MonsterMoveStateMachine(list, state);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState defaultState = new AnimState("Default");
        AnimState dieState = new AnimState("Die");

        CreatureAnimator creatureAnimator = new CreatureAnimator(defaultState, controller);
        creatureAnimator.AddAnyState("Dead", dieState);

        return creatureAnimator;
    }
}
