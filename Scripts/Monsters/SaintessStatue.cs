using ArknightsMap.Scripts.Powers;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Monsters;

[RegisterMonster]
public class SaintessStatue : ModMonsterTemplate
{
    public override int MinInitialHp => 99999999;
    public override int MaxInitialHp => MinInitialHp;

    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath: $"res://ArknightsMap/scenes/monsters/{GetType().Name}.tscn");

    public override async Task AfterAddedToRoom()
    {
        Creature.HpDisplay = HpDisplay.InfiniteWithoutNumbers;
        SanctityPower sanctityPower = (SanctityPower)ModelDb.Power<SanctityPower>().ToMutable();
        sanctityPower.SetDirection(Creature.SlotName == "L" ? 1 : -1);
        await PowerCmd.Apply(new ThrowingPlayerChoiceContext(), sanctityPower, Creature, 1, Creature, null);
        if (Creature.SlotName == "R")
        {
            Sprite2D visuals = Creature.GetCreatureNode()!.Visuals.GetNode<Sprite2D>("Visuals");
            visuals.Scale = new Vector2(-0.2f, 0.2f);
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> list = new List<MonsterState>();
        MoveState noneState = new MoveState("NONE", async targets => { }, new UnknownIntent());
        noneState.FollowUpState = noneState;

        list.Add(noneState);

        return new MonsterMoveStateMachine(list, noneState);
    }
}
