using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Relics;
using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Ancients;

[RegisterSharedAncient]
public class Bagpipe : ModAncientEventTemplate
{
    // 选项按钮颜色
    public override Color ButtonColor => new(0.12f, 0.2f, 0.8f, 0.5f);

    // 对话框颜色
    public override Color DialogueColor => new(0.12f, 0.2f, 0.8f);

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Beer", 0)];

    // 自定义场景的路径
    public override EventAssetProfile AssetProfile => new(BackgroundScenePath: "res://ArknightsMap/scenes/ancients/Bagpipe.tscn");

    // 自定义地图图标和轮廓的路径
    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile =>
        new(
            MapIconPath: "res://icon.svg",
            MapIconOutlinePath: "res://icon.svg",
            RunHistoryIconPath: "res://ArknightsMap/images/ancients/Bagpipe/avatar.png",
            RunHistoryIconOutlinePath: "res://ArknightsMap/images/ancients/Bagpipe/avatar.png"
        );

    public override void CalculateVars()
    {
        var store = RitsuLibFramework.GetDataStore(Entry.ModId);
        DynamicVars["Beer"].BaseValue = store.Get<WheatBeerCounter>("wheatbeercounter").Value;
    }

    // 所有可能的选项
    public override IEnumerable<EventOption> AllPossibleOptions =>
        [
            ..SinglePlayerChoice, 
            RelicOption<OfferAssistance>(), //施以援手
        ];

    public IEnumerable<EventOption> SinglePlayerChoice =>
        [
            RelicOption<HighImpactAssault>(), //高效冲击
            RelicOption<TheSpear>(), //破城矛
            RelicOption<LockedBreechBurst>(), //闭膛连发
            RelicOption<LandenBeer>(), //兰登佳酿
            RelicOption<PreciseReloading>(), //精密填弹
            RelicOption<EjectionBullet>(), //弹射弹药
            RelicOption<Haystack>(), //干草垛
            RelicOption<WheatEar>(), //麦穗
            RelicOption<StarrySkyPhoto>(), //星空照片
            RelicOption<MartialTradition>(), //军事传统
        ];

    // 生成选项
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> list = SinglePlayerChoice.ToList();
        list.UnstableShuffle(Rng);
        list = list.Take(3).ToList();
        if (Owner!.RunState.Players.Count > 1)
        {
            list = list.Take(2).ToList();
            list.Add(RelicOption<OfferAssistance>());
        }
        return list;
    }

    public override bool IsValidForAct(ActModel act)
    {
        return act is Wilds;
    }
}
