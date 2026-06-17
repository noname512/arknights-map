using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Encounters;

[RegisterActEncounter(typeof(Wilds))]
public class AshyMarsh : AbstractWildsEncounter
{
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<DublinnFlamerazer>(), ModelDb.Monster<DublinnCannoneer>()];

    public override bool IsWeak => false;

    public override EncounterAssetProfile AssetProfile => new(EncounterScenePath: $"res://ArknightsMap/scenes/encounters/{GetType().Name}.tscn");

    public override string CustomBgm => Random.Shared.Next(2) == 1 ? "event:/ArknightsMap/music/fblw_bat" : "event:/ArknightsMap/music/zwyh_bat";

    // 怪物槽位的名字
    public override IReadOnlyList<string> Slots => ["first", "second"];

    public override RoomType RoomType => RoomType.Monster;

    // 如果你的场景太大，可以调整缩放。此外还可以使用 GetCameraOffset 来调整摄像机位置
    // public override float GetCameraScaling() => 0.8f;
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
        [(ModelDb.Monster<DublinnFlamerazer>().ToMutable(), "first"), (ModelDb.Monster<DublinnCannoneer>().ToMutable(), "second")];
}
