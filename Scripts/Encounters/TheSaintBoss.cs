using ArknightsMap.Scripts.Encounters;
using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Test.Scripts;

[RegisterActEncounter(typeof(Glory))]
public class TheSaintBoss : AbstractLateranoEncounter
{
    // 所有可能出现的怪物
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<TheSaint>()];

    public override EncounterAssetProfile AssetProfile => new(EncounterScenePath: $"res://ArknightsMap/scenes/encounters/{GetType().Name}.tscn");
    public override RoomType RoomType => RoomType.Boss; // 这个遭遇的房间类型，这里是boss怪物

public override IReadOnlyList<string> Slots => ["first", "second", "third"];
    // 不要忘了这里的model需要调用ToMutable()，表示不是标准值而是战斗中的可变数据
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [
        (ModelDb.Monster<TheSaint>().ToMutable(), "second") // 如果不想指定怪物生成在哪个槽位，可以直接传null，系统会自动分配
    ];

    // 可选的生成条件，例如只能在密林生成
    // public override bool IsValidForAct(ActModel act)
    // {
    //     return act is Overgrowth;
    // }
}