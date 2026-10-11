using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Relics;
using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Ancients;

[RegisterSharedAncient]
public class Pramanix : ModAncientEventTemplate
{
    // 选项按钮颜色
    public override Color ButtonColor => new(0.14f, 0.6f, 0.72f, 0.5f);

    // 对话框颜色
    public override Color DialogueColor => new(0.17f, 0.75f, 0.9f);

    // 自定义场景的路径
    public override EventAssetProfile AssetProfile => new(BackgroundScenePath: $"res://ArknightsMap/scenes/ancients/{GetType().Name}.tscn");

    // 自定义地图图标和轮廓的路径
    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile =>
        new(
            MapIconPath: "res://icon.svg", //$"res://ArknightsMap/images/ancients/{GetType().Name}/icon.png",
            MapIconOutlinePath: "res://icon.svg", //$"res://ArknightsMap/images/ancients/{GetType().Name}/icon_outline.png",
            RunHistoryIconPath: $"res://ArknightsMap/images/ancients/{GetType().Name}/avatar.png",
            RunHistoryIconOutlinePath: $"res://ArknightsMap/images/ancients/{GetType().Name}/avatar.png"
        );

    public IEnumerable<EventOption> SinglePlayerOptions =>
        [
            CreateModRelicOption<HerAllowance>(), //祂的许可
            CreateModRelicOption<EreSnowBellsChime>(), //铃音吹雪
            CreateModRelicOption<Faith>(), //信仰
            CreateModRelicOption<Pilgrimage>(), //圣巡
            CreateModRelicOption<SnowTracks>(), //雪迹
            CreateModRelicOption<ByKjeragandrPramanix>(), //耶拉冈德在上·初雪
            CreateModRelicOption<PeaksCladInFrost>(), //霜涛覆岭
            CreateModRelicOption<TowardsTheMountainBow>(), //群山俯首
            CreateModRelicOption<BlessingOfKarlan>(), //圣山的祝福
            CreateModRelicOption<NatureDeterrent>(), //自然威慑
        ];

    // 所有可能的选项
    public override IEnumerable<EventOption> AllPossibleOptions =>
        [
            .. SinglePlayerOptions,
            CreateModRelicOption<TriClanCouncil>(), //三族议会
        ];

    // 生成选项
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> list = SinglePlayerOptions.ToList();
        if (Owner!.RunState.Players.Count > 1)
        {
            list.Add(CreateModRelicOption<TriClanCouncil>());
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
