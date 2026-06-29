using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Encounters;

[RegisterActEncounter(typeof(SnowyMountain))]
public class TschaggattasElite : AbstractSnowyMountainEncounter
{
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<Tschaggatta>()];

    public override EncounterAssetProfile AssetProfile => new(EncounterScenePath: $"res://ArknightsMap/scenes/encounters/{GetType().Name}.tscn");

    // public override string CustomBgm => "event:/ArknightsMap/music/fblw_bat";

    public override IReadOnlyList<string> Slots => ["first", "second", "third", "fourth"];

    public override RoomType RoomType => RoomType.Elite;

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
        [
            (ModelDb.Monster<Tschaggatta>().ToMutable(), "first"),
            (ModelDb.Monster<Tschaggatta>().ToMutable(), "second"),
            (ModelDb.Monster<Tschaggatta>().ToMutable(), "third"),
            (ModelDb.Monster<Tschaggatta>().ToMutable(), "fourth"),
        ];
}
