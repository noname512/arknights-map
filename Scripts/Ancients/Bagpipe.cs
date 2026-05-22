using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Relics;
using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace ArknightsMap.Scripts.Ancients;

[RegisterSharedAncient]
public class Bagpipe : ModAncientEventTemplate
{
    // 选项按钮颜色
    public override Color ButtonColor => new(0.12f, 0.2f, 0.8f, 0.5f);
    // 对话框颜色
    public override Color DialogueColor => new(0.12f, 0.2f, 0.8f);
    // 自定义场景的路径
    public override EventAssetProfile AssetProfile => new(
        BackgroundScenePath: "res://ArknightsMap/scenes/ancients/Reed.tscn"     //TODO
    );

    // 自定义地图图标和轮廓的路径
    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile => new(
        MapIconPath: "res://icon.svg",
        MapIconOutlinePath: "res://icon.svg",
        RunHistoryIconPath: "res://ArknightsMap/images/ancients/Bagpipe/avatar.png",
        RunHistoryIconOutlinePath: "res://ArknightsMap/images/ancients/Bagpipe/avatar.png"
    );

    // 所有可能的选项
    public override IEnumerable<EventOption> AllPossibleOptions => [
        RelicOption<HighImpactAssault>(),       //高效冲击
        RelicOption<TheSpear>(),                //破城矛
        RelicOption<LockedBreechBurst>(),       //闭膛连发
        RelicOption<LandenBeer>(),              //兰登佳酿
        RelicOption<PreciseReloading>(),        //精密填弹
        RelicOption<EjectionBullet>(),          //弹射弹药
    ];

    // 生成选项
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> list = AllPossibleOptions.ToList();
        list.UnstableShuffle(Rng);
        return list.Take(3).ToList();        
    }

    // 出现条件。这里是只能在第二幕出现（索引为1）
    public override bool IsAllowed(IRunState runState)
    {
        return runState.Acts[1] is Wilds && runState.CurrentActIndex == 1;
    }
}