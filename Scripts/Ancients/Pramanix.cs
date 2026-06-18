using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Relics;
using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Ancients;

[RegisterSharedAncient]
public class Pramanix : ModAncientEventTemplate
{
    // 选项按钮颜色
    public override Color ButtonColor => new(0.12f, 0.2f, 0.8f, 0.5f);

    // 对话框颜色
    public override Color DialogueColor => new(0.12f, 0.2f, 0.8f);

    // 自定义场景的路径
    public override EventAssetProfile AssetProfile => new(BackgroundScenePath: "res://ArknightsMap/scenes/ancients/Pramanix.tscn");

    // 自定义地图图标和轮廓的路径
    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile =>
        new(
            MapIconPath: "res://icon.svg",
            MapIconOutlinePath: "res://icon.svg",
            RunHistoryIconPath: "res://ArknightsMap/images/ancients/Pramanix/avatar.png",
            RunHistoryIconOutlinePath: "res://ArknightsMap/images/ancients/Pramanix/avatar.png"
        );

    public IEnumerable<EventOption> SinglePlayerOptions =>
    [
        RelicOption<HerAllowance>(), //祂的许可
        RelicOption<EreSnowBellsChime>(), //铃音吹雪
        RelicOption<Faith>(), //信仰
        RelicOption<Pilgrimage>(), //圣巡
        // RelicOption<SnowTracks>(),           //雪迹
        RelicOption<ByKjeragandrPramanix>(), //耶拉冈德在上·初雪
        RelicOption<PeaksCladInForest>(), //霜涛覆岭
        RelicOption<TowardsTheMountainBow>(), //群山俯首
        RelicOption<BlessingOfKarlan>(), //圣山的祝福
        RelicOption<NatureDeterrent>(), //自然威慑
    ];
    // 所有可能的选项
    public override IEnumerable<EventOption> AllPossibleOptions => [
        .. SinglePlayerOptions, 
        RelicOption<TriClanCouncil>(),          //三族议会
        
    ];

    // 生成选项
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> list = SinglePlayerOptions.ToList();
        if (Owner!.RunState.Players.Count > 1)
        {
            list.Add(RelicOption<TriClanCouncil>());
        }
        list.UnstableShuffle(Rng);
        list = list.Take(3).ToList();
        return list;
    }

    public override bool IsValidForAct(ActModel act)
    {
        return act is SnowyMountain;
    }
}
