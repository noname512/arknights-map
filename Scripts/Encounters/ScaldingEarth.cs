using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Encounters;

[RegisterActEncounter(typeof(Wilds))]
public class ScaldingEarth : ModEncounterTemplate
{
    // 所有可能出现的怪物
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<Fireball>()];

    // 这个遭遇是否是弱怪池
    public override bool IsWeak => false;

    // 遭遇场景（用来指定每个怪物站哪）
    public override EncounterAssetProfile AssetProfile => new(
        EncounterScenePath: $"res://ArknightsMap/scenes/encounters/ScorchingDream.tscn"
    );

    // 怪物槽位的名字
    public override IReadOnlyList<string> Slots => [
        "first",
        "second",
        "third"
    ];

    public override RoomType RoomType => RoomType.Monster; // 这个遭遇的房间类型，这里是普通怪物

    // 如果你的场景太大，可以调整缩放。此外还可以使用 GetCameraOffset 来调整摄像机位置
    // public override float GetCameraScaling() => 0.8f;

    // 生成怪物列表
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [
        (ModelDb.Monster<DublinnEvocator>().ToMutable(), "first"),
        (ModelDb.Monster<Fireball>().ToMutable(), "second"),
    ];
}