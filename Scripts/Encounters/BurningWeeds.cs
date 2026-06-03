using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Encounters;

[RegisterActEncounter(typeof(Wilds))]
public class BurningWeeds : AbstractWildsEncounter
{
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<DublinnSpecOps>()];

    public override bool IsWeak => false;

    public override EncounterAssetProfile AssetProfile => new(
        RunHistoryIconPath: $"res://ArknightsMap/images/map/{GetType().Name}History.png",
        RunHistoryIconOutlinePath: $"res://ArknightsMap/images/map/{GetType().Name}History_outline.png",
        EncounterScenePath: $"res://ArknightsMap/scenes/encounters/{GetType().Name}.tscn"
    );

    public override IReadOnlyList<string> Slots => [
        "first"
    ];

    public override RoomType RoomType => RoomType.Monster;

    // 如果你的场景太大，可以调整缩放。此外还可以使用 GetCameraOffset 来调整摄像机位置
    // public override float GetCameraScaling() => 0.8f;

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [
        (ModelDb.Monster<DublinnSpecOps>().ToMutable(), "first")
    ];
}