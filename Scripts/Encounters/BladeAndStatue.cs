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
public class BladeAndStatue : AbstractLateranoEncounter
{
    // 所有可能出现的怪物
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<SanktaBlade>(), ModelDb.Monster<SanktaStatue>()];

    public override EncounterAssetProfile AssetProfile => new(EncounterScenePath: $"res://ArknightsMap/scenes/encounters/{GetType().Name}.tscn");
    public override bool IsWeak => false;
    public override RoomType RoomType => RoomType.Monster; // 这个遭遇的房间类型，这里是普通怪物

    public override string CustomBgm => "event:/ArknightsMap/music/zsxj_bat_1";

    public override IReadOnlyList<string> Slots => ["first", "second"];



    // 不要忘了这里的model需要调用ToMutable()，表示不是标准值而是战斗中的可变数据
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [
        (ModelDb.Monster<SanktaBlade>().ToMutable(), "first"), 
        (ModelDb.Monster<SanktaStatue>().ToMutable(), "second")
    ];

    // 可选的生成条件，例如只能在密林生成
    // public override bool IsValidForAct(ActModel act)
    // {
    //     return act is Overgrowth;
    // }
}