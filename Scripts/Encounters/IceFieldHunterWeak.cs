using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ArknightsMap.Scripts.Encounters;

[RegisterActEncounter(typeof(SnowyMountain))]
public class IceFieldHunterWeak : AbstractSnowyMountainEncounter
{
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<IcefieldHunter>()];

    public override string CustomBgm => "event:/ArknightsMap/music/fxgj_bat_1";

    public override IReadOnlyList<string> Slots => ["5", "7"];

    public override RoomType RoomType => RoomType.Monster;
    public override bool IsWeak => true;

    // 如果你的场景太大，可以调整缩放。此外还可以使用 GetCameraOffset 来调整摄像机位置
    // public override float GetCameraScaling() => 0.8f;

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
        [
            (ModelDb.Monster<IcefieldHunter>().ToMutable(), "5"), // 防折叠
            (ModelDb.Monster<IcefieldHunter>().ToMutable(), "7"),
        ];
}
