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
public class SilverAsh : ModAncientEventTemplate
{
    // 选项按钮颜色
    public override Color ButtonColor => new(0.16f, 0.16f, 0.16f, 0.5f);

    // 对话框颜色
    public override Color DialogueColor => new(0.2f, 0.2f, 0.2f);

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
            RelicOption<Tenzin>(), // 丹增
            RelicOption<OpenGame>(), // 开放性开局
            RelicOption<FixedTurret>(), // 定点炮台
            RelicOption<Kd4>(), // 以身入局
        ];

    // 所有可能的选项
    public override IEnumerable<EventOption> AllPossibleOptions =>
        [
            .. SinglePlayerOptions,
            RelicOption<TriClanCouncil>(), // 三族议会
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
