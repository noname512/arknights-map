using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Relics;
using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Ancients;

[RegisterSharedAncient]
public class Gracebearer : ModAncientEventTemplate
{
    // 选项按钮颜色
    public override Color ButtonColor => new(0.47f, 0.24f, 0.29f, 0.5f);

    // 对话框颜色
    public override Color DialogueColor => new(0.47f, 0.24f, 0.29f);

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    // 自定义场景的路径
    public override EventAssetProfile AssetProfile => new(BackgroundScenePath: $"res://ArknightsMap/scenes/ancients/{GetType().Name}.tscn");

    // 自定义地图图标和轮廓的路径
    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile =>
        new(
            MapIconPath: $"res://ArknightsMap/images/ancients/{GetType().Name}/icon.png",
            MapIconOutlinePath: $"res://ArknightsMap/images/ancients/{GetType().Name}/icon_outline.png",
            RunHistoryIconPath: $"res://ArknightsMap/images/ancients/{GetType().Name}/avatar.png",
            RunHistoryIconOutlinePath: $"res://ArknightsMap/images/ancients/{GetType().Name}/avatar.png"
        );

    // 所有可能的选项
    public override IEnumerable<EventOption> AllPossibleOptions =>
        [
            .. SinglePlayerChoice,
            CreateModRelicOption<NunHabit>(), //修女服
        ];

    public IEnumerable<EventOption> SinglePlayerChoice =>
        [
            CreateModRelicOption<BloodBurst>(), //血性爆发
            CreateModRelicOption<Lullaby>(), //安眠曲
            CreateModRelicOption<PrayInvite>(), //祈祷邀约
            CreateModRelicOption<ActAs>(), //“扮演”
            CreateModRelicOption<OpportuneMercy>(), // 趁势怜悯
            CreateModRelicOption<BloodedDress>(), // 染血的裙子
            CreateModRelicOption<Luckily>(), // “幸运”
            CreateModRelicOption<BreadWithSugar>(), // 加糖面包
            CreateModRelicOption<Gospel>(), // 福音
            CreateModRelicOption<SkullcrackingElegy>(), // 开颅挽歌
        ];

    // 生成选项
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> list;
        if (Owner!.RunState.Players.Count > 1)
        {
            list = AllPossibleOptions.ToList();
        }
        else
        {
            list = SinglePlayerChoice.ToList();
        }
        list.UnstableShuffle(Rng);
        list = list.Take(3).ToList();
        return list;
    }

    public override bool IsValidForAct(ActModel act)
    {
        return act is Laterano;
    }
}
