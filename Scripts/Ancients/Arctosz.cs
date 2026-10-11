using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Relics;
using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Ancients;

[RegisterSharedAncient]
public class Arctosz : ModAncientEventTemplate
{
    // 选项按钮颜色
    public override Color ButtonColor => new(0.17f, 0.13f, 0.13f, 0.5f);

    // 对话框颜色
    public override Color DialogueColor => new(0.21f, 0.17f, 0.17f);

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

    private IEnumerable<EventOption> Pool1Single =>
        [
            CreateModRelicOption<ByKjeragandrArctosz>(), // 耶拉冈德在上·阿克托斯
            CreateModRelicOption<AncientTome>(), // 旧典籍
            CreateModRelicOption<CollectionRoom>(), // 收藏室
            CreateModRelicOption<PlanSheet>(), // 计划单
        ];
    private IEnumerable<EventOption> Pool2 =>
        [
            CreateModRelicOption<MightyFrame>(), // 蛮力之躯
            CreateModRelicOption<HuntingTools>(), // 狩猎工具
            CreateModRelicOption<WeightedPlate>(), // 配重片
            CreateModRelicOption<BlizzardCrossbow>(), // 风雪连弩
        ];
    private IEnumerable<EventOption> Pool3 =>
        [
            CreateModRelicOption<OffensiveInstinct>(), // 进攻本能
            CreateModRelicOption<AncientHeritage>(), // 古老传承
            CreateModRelicOption<Overexertion>(), // 超负荷
        ];

    // 所有可能的选项
    public override IEnumerable<EventOption> AllPossibleOptions => [.. Pool1Single, .. Pool2, .. Pool3, CreateModRelicOption<TriClanCouncil>()];

    // 生成选项
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> Pool1 = Pool1Single.ToList();
        if (Owner!.RunState.Players.Count > 1)
        {
            Pool1.Add(CreateModRelicOption<TriClanCouncil>());
        }
        return [Rng.NextItem(Pool1)!, Rng.NextItem(Pool2)!, Rng.NextItem(Pool3)!];
    }

    public override bool IsValidForAct(ActModel act)
    {
        return act is SnowyMountain;
    }
}
