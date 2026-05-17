using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Encounters;

[RegisterActEncounter(typeof(Glory))]
public class DublinnPhalanx : ModEncounterTemplate
{
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<DublinnPhalanxInfantry>()];

    public override bool IsWeak => true;

    public override EncounterAssetProfile AssetProfile => new(
        EncounterScenePath: "res://ArknightsMap/scenes/encounters/DublinnPhalanx.tscn"
    );

    public override IReadOnlyList<string> Slots => [
        "first",
        "second"
    ];

    public override RoomType RoomType => RoomType.Monster;

    // 如果你的场景太大，可以调整缩放。此外还可以使用 GetCameraOffset 来调整摄像机位置
    // public override float GetCameraScaling() => 0.8f;

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [
        (ModelDb.Monster<DublinnPhalanxInfantry>().ToMutable(), "first"),
        (ModelDb.Monster<DublinnPhalanxInfantry>().ToMutable(), "second")
    ];
}