using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Encounters;

[RegisterActEncounter(typeof(Wilds))]
public class MarchOfTheDead : AbstractWildsEncounter
{
    public override bool isBurningAtStart => true;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<DublinnFlamechaserGuard>()
    ];

    public override EncounterAssetProfile AssetProfile => new(
        EncounterScenePath: $"res://ArknightsMap/scenes/encounters/{GetType().Name}.tscn"
    );

    public override string CustomBgm => "event:/ArknightsMap/music/zwyh_bat";

    public override IReadOnlyList<string> Slots => [
        "first",
    ];

    public override RoomType RoomType => RoomType.Elite;

    // 如果你的场景太大，可以调整缩放。此外还可以使用 GetCameraOffset 来调整摄像机位置
    // public override float GetCameraScaling() => 0.8f;

    // 生成怪物列表
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [
        (ModelDb.Monster<DublinnFlamechaserGuard>().ToMutable(), "first"),
    ];
}