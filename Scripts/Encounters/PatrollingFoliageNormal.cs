using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Encounters;

[RegisterActEncounter(typeof(Wilds))]
public class PatrollingFoliageNormal : MyAbstractEncounter
{
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<PatrollingFoliage>()];

    public override bool IsWeak => true;

    public override EncounterAssetProfile AssetProfile => new(
        EncounterScenePath: $"res://ArknightsMap/scenes/encounters/{GetType().Name}.tscn"
    );

    public override IReadOnlyList<string> Slots => [
        "first",
        "seed1",
        "seed2",
        "seed3"
    ];

    public override RoomType RoomType => RoomType.Monster;

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [
        (ModelDb.Monster<PatrollingFoliage>().ToMutable(), "first")
    ];
}