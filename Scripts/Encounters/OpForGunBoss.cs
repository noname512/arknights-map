using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Encounters;
using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Encounters;

[RegisterActEncounter(typeof(Laterano))]
public class OpForGunBoss : AbstractLateranoEncounter
{
    // 所有可能出现的怪物
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<OpForGun>(), ModelDb.Monster<OpCar>()];

    public override EncounterAssetProfile AssetProfile =>
        new(
            RunHistoryIconPath: $"res://ArknightsMap/images/map/{GetType().Name}History.png",
            RunHistoryIconOutlinePath: $"res://ArknightsMap/images/map/{GetType().Name}History_outline.png",
            EncounterScenePath: $"res://ArknightsMap/scenes/encounters/{GetType().Name}.tscn"
        );

    public override string BossNodePath => $"res://ArknightsMap/images/map/{GetType().Name}Icon";

    public override string CustomBgm => "event:/ArknightsMap/music/op_for_gun_bat";

    public override bool FullyCenterPlayers => true;

    public override RoomType RoomType => RoomType.Boss; // 这个遭遇的房间类型，这里是boss怪物

    public override IReadOnlyList<string> Slots => ["first_left","first_right", "second_left", "second_right", "boss_left", "boss_right"];

    // 不要忘了这里的model需要调用ToMutable()，表示不是标准值而是战斗中的可变数据
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() =>
        [
            (ModelDb.Monster<OpForGun>().ToMutable(), "boss_right"), 
            (ModelDb.Monster<OpCar>().ToMutable(), "second_left"),
            (ModelDb.Monster<OpCar>().ToMutable(), "second_right"),
        ];

    // 可选的生成条件，例如只能在密林生成
    // public override bool IsValidForAct(ActModel act)
    // {
    //     return act is Overgrowth;
    // }
}
