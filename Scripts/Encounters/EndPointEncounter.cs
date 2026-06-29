using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Encounters;

[RegisterGlobalEncounter]
public class EndPointEncounter : AbstractWildsEncounter
{
    public override IEnumerable<MonsterModel> AllPossibleMonsters =>
        [
            ModelDb.Monster<EndPoint>(),
            ModelDb.Monster<BurningVine>(),
            ModelDb.Monster<CabbageSeedling>(),
            ModelDb.Monster<PatrollingFoliage>(),
            ModelDb.Monster<TreeShield>(),
            ModelDb.Monster<AshCreation>(),
        ];

    public override bool IsValidForAct(ActModel act) => false;

    protected override bool UseProgrammaticCombatBackground => true;

    public override EncounterAssetProfile AssetProfile =>
        new(
            EncounterScenePath: $"res://ArknightsMap/scenes/encounters/{GetType().Name}.tscn",
            BackgroundScenePath: "res://ArknightsMap/scenes/acts/Wilds/wilds_background.tscn",
            BackgroundLayersDirectoryPath: "res://ArknightsMap/scenes/acts/Wilds/layers"
        );

    public override string CustomBgm => "event:/ArknightsMap/music/wgrsdj_bat";

    public override IReadOnlyList<string> Slots =>
        ["EndPoint", "seed0", "seed1", "seed2", "seed3", "BurningVine", "PatrollingFoliage", "TreeShield", "AshCreation"];

    public override RoomType RoomType => RoomType.Elite;

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
        [(ModelDb.Monster<CabbageSeedling>().ToMutable(), "seed0"), (ModelDb.Monster<EndPoint>().ToMutable(), "EndPoint")];
}
