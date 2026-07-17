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
public class Paganini : ModAncientEventTemplate
{
    // 选项按钮颜色
    public override Color ButtonColor => new(0.12f, 0.2f, 0.8f, 0.5f);

    // 对话框颜色
    public override Color DialogueColor => new(0.12f, 0.2f, 0.8f);

    // 自定义场景的路径
    public override EventAssetProfile AssetProfile => new(BackgroundScenePath: "res://ArknightsMap/scenes/ancients/Paganini.tscn");

    // 自定义地图图标和轮廓的路径
    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile =>
        new(
            MapIconPath: "res://ArknightsMap/images/ancients/Paganini/icon.png",
            MapIconOutlinePath: "res://ArknightsMap/images/ancients/Paganini/icon_outline.png",
            RunHistoryIconPath: "res://ArknightsMap/images/ancients/Paganini/avatar.png",
            RunHistoryIconOutlinePath: "res://ArknightsMap/images/ancients/Paganini/avatar.png"
        );

    private IReadOnlyList<EventOption> Pool1 =>
        [
            CreateModRelicOption<CustomMade>(), // 定制铳械
            CreateModRelicOption<Solvent>(), // 铳械清洗溶剂
            CreateModRelicOption<WeiLaterano>(), // 盟约：拉特兰
        ];
    private IReadOnlyList<EventOption> Pool2 =>
        [
            CreateModRelicOption<GiantGun>(), // 铳骑的巨铳
            CreateModRelicOption<NoSugarIce>(), // 无糖冰淇淋
            
        ];
    private IReadOnlyList<EventOption> Pool3 =>
        [
            CreateModRelicOption<Aphasia>(), // “失语”
            CreateModRelicOption<Vannini>(), // 梵妮的铳
            CreateModRelicOption<Wrap>(), // 披肩
            
        ];

    // 所有可能的选项
    public override IEnumerable<EventOption> AllPossibleOptions => [.. Pool1, .. Pool2, .. Pool3];

    // 生成选项
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return [Rng.NextItem(Pool1)!, Rng.NextItem(Pool2)!, Rng.NextItem(Pool3)!];
    }

    
}