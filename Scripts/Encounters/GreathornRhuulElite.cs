using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Encounters;

[RegisterActEncounter(typeof(SnowyMountain))]
public class GreathornRhuulElite : AbstractSnowyMountainEncounter
{
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<GreathornRhuul>()];

    public override string CustomBgm => "event:/ArknightsMap/music/all_flames_returned_bat_1";
    public override int playerStartPosition => 3;
    public override int windBlowDirection => -1;
    public override int windBlowTurn => 3;

    public override IReadOnlyList<string> Slots => ["6"];

    public override RoomType RoomType => RoomType.Elite;
    public override bool IsWeak => false;

    // 如果你的场景太大，可以调整缩放。此外还可以使用 GetCameraOffset 来调整摄像机位置
    // public override float GetCameraScaling() => 0.8f;

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
        [
            (ModelDb.Monster<GreathornRhuul>().ToMutable(), "6"), // 防折叠
        ];
}
