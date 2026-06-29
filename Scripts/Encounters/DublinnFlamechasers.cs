using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Encounters;

[RegisterGlobalEncounter]
public class DublinnFlamechasers : AbstractWildsEncounter
{
    public override bool isBurningAtStart => true;
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<DublinnFlamechaserGuard>(), ModelDb.Monster<DublinnFlamechaserSoldier>()];

    public override bool IsValidForAct(ActModel act) => false;

    protected override bool UseProgrammaticCombatBackground => true;

    public override EncounterAssetProfile AssetProfile =>
        new(
            EncounterScenePath: $"res://ArknightsMap/scenes/encounters/{GetType().Name}.tscn",
            BackgroundScenePath: "res://ArknightsMap/scenes/acts/Wilds/wilds_background.tscn",
            BackgroundLayersDirectoryPath: "res://ArknightsMap/scenes/acts/Wilds/layers"
        );

    public override string CustomBgm => "event:/ArknightsMap/music/zwyh_bat";

    public override IReadOnlyList<string> Slots => ["first", "second", "third", "fourth"];

    public override RoomType RoomType => RoomType.Elite;

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
        [
            (ModelDb.Monster<DublinnFlamechaserGuard>().ToMutable(), "first"),
            (ModelDb.Monster<DublinnFlamechaserSoldier>().ToMutable(), "second"),
            (ModelDb.Monster<DublinnFlamechaserSoldier>().ToMutable(), "third"),
            (ModelDb.Monster<DublinnFlamechaserSoldier>().ToMutable(), "fourth"),
        ];
}
