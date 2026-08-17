using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Relics;
using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using STS2RitsuLib;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Ancients;

[RegisterSharedAncient]
public class Gracebearer : ModAncientEventTemplate
{
    // 选项按钮颜色
    public override Color ButtonColor => new(0.12f, 0.2f, 0.8f, 0.5f);

    // 对话框颜色
    public override Color DialogueColor => new(0.12f, 0.2f, 0.8f);

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    // 自定义场景的路径
    public override EventAssetProfile AssetProfile => new(BackgroundScenePath: "res://ArknightsMap/scenes/ancients/Bagpipe.tscn");

    // 自定义地图图标和轮廓的路径
    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile =>
        new(
            MapIconPath: "res://ArknightsMap/images/ancients/Bagpipe/icon.png",
            MapIconOutlinePath: "res://ArknightsMap/images/ancients/Bagpipe/icon_outline.png",
            RunHistoryIconPath: "res://ArknightsMap/images/ancients/Bagpipe/avatar.png",
            RunHistoryIconOutlinePath: "res://ArknightsMap/images/ancients/Bagpipe/avatar.png"
            // TODO
        );


    // 所有可能的选项
    public override IEnumerable<EventOption> AllPossibleOptions =>
        [
            .. SinglePlayerChoice,
            RelicOption<OfferAssistance>(), //施以援手
        ];

    public IEnumerable<EventOption> SinglePlayerChoice =>
        [
            RelicOption<BloodBurst>(), //血性爆发
            RelicOption<Lullaby>(), //安眠曲
            RelicOption<PrayInvite>(), //祈祷邀约
            RelicOption<ActAs>(), //“扮演”
            RelicOption<OpportuneMercy>(), // 趁势怜悯
        ];

    // 生成选项
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> list = SinglePlayerChoice.ToList();
        list.UnstableShuffle(Rng);
        list = list.Take(3).ToList();
        return list;
    }

    public override bool IsValidForAct(ActModel act)
    {
        return act is Laterano;
    }
}
