using ArknightsMap.Scripts.Encounters;
using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Test.Scripts;

[RegisterActEncounter(typeof(Glory))]
public class TwoWarriors : AbstractLateranoEncounter
{

    public override EncounterAssetProfile AssetProfile => new(EncounterScenePath: $"res://ArknightsMap/scenes/encounters/{GetType().Name}.tscn");
    // 所有可能出现的怪物
    public override IEnumerable<MonsterModel> AllPossibleMonsters => 
    [ModelDb.Monster<PathfinderWarrior>()];
    public override RoomType RoomType => RoomType.Monster; // 这个遭遇的房间类型，这里是精英怪物
    public override bool IsWeak => true;
    public override IReadOnlyList<string> Slots => ["first", "second"];

    // 不要忘了这里的model需要调用ToMutable()，表示不是标准值而是战斗中的可变数据
    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [
        (ModelDb.Monster<PathfinderWarrior>().ToMutable(), "first"),
        (ModelDb.Monster<PathfinderWarrior>().ToMutable(), "second"),
        
    ];

    // 可选的生成条件，例如只能在密林生成
    // public override bool IsValidForAct(ActModel act)
    // {
    //     return act is Overgrowth;
    // }
}