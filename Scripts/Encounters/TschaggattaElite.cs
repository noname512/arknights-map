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
    public override string CustomBgm => "event:/ArknightsMap/music/fxgj_bat_2";

    public override IReadOnlyList<string> Slots => ["5", "6", "7", "8"];

    public override RoomType RoomType => RoomType.Elite;

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
        [
            (ModelDb.Monster<Tschaggatta>().ToMutable(), "5"),
            (ModelDb.Monster<Tschaggatta>().ToMutable(), "6"),
            (ModelDb.Monster<Tschaggatta>().ToMutable(), "7"),
            (ModelDb.Monster<Tschaggatta>().ToMutable(), "8"),
        ];
}
