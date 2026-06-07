using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Relics;
using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace ArknightsMap.Scripts.Ancients;

[RegisterSharedAncient]
public class Reed : ModAncientEventTemplate
{
    // 选项按钮颜色
    public override Color ButtonColor => new(0.12f, 0.2f, 0.8f, 0.5f);
    // 对话框颜色
    public override Color DialogueColor => new(0.12f, 0.2f, 0.8f);
    // 自定义场景的路径
    public override EventAssetProfile AssetProfile => new(
        BackgroundScenePath: "res://ArknightsMap/scenes/ancients/Reed.tscn"
    );

    // 自定义地图图标和轮廓的路径
    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile => new(
        MapIconPath: "res://icon.svg",
        MapIconOutlinePath: "res://icon.svg",
        RunHistoryIconPath: "res://ArknightsMap/images/ancients/Reed/avatar.png",
        RunHistoryIconOutlinePath: "res://ArknightsMap/images/ancients/Reed/avatar.png"
    );

    // 固定池一和二
    private IReadOnlyList<EventOption> Pool1 => [
            CreateModRelicOption<BurnScar>(),   // 灼痕
            CreateModRelicOption<Hope>(),       // 希望
            CreateModRelicOption<MarkOfWither>(), // 枯萎印记
        ];
    private IReadOnlyList<EventOption> Pool2 => [
            CreateModRelicOption<SoulSpark>(),  // 生灵火花
            CreateModRelicOption<LiveFlame>(),  // 活化火苗
            CreateModRelicOption<MarkOfTara>(),  // 塔拉印记
            CreateModRelicOption<WitherAndThrive>(),  // 枯荣共息
        ];

    // 带权重池三。权重越大越有机会生成。当然你也可以写自定义的列表生成函数
    private WeightedList<EventOption> Pool3 => new()
    {
        { CreateModRelicOption<AidOfLeader>(), 1 }, // “领袖”的援助
        { CreateModRelicOption<Kindling>(), 1 }, // 火种
        { CreateModRelicOption<BurnItAll>(), 1 }, // 燃烧殆尽
        { CreateModRelicOption<WildfireSpread>(), 1 }, // 燃烧殆尽
    };

    // 所有可能的选项
    public override IEnumerable<EventOption> AllPossibleOptions => [.. Pool1, .. Pool2, .. Pool3];

    // 生成选项
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return
        [
            Rng.NextItem(Pool1)!,
            Rng.NextItem(Pool2)!,
            Pool3.GetRandom(Rng),
        ];
    }

    public override bool IsValidForAct(ActModel act)
    {
        return act is Wilds;
    }
}