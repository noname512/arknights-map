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
public class Lemuen : ModAncientEventTemplate
{
    // 选项按钮颜色
<<<<<<< Updated upstream
    public override Color ButtonColor => new Color(0.76f, 0.46f, 0.54f, 0.5f);
=======
    public override Color ButtonColor => new Color(0.95f, 0.58f, 0.68f, 0.5f);
>>>>>>> Stashed changes

    // 对话框颜色
    public override Color DialogueColor => new Color(0.95f, 0.58f, 0.68f);

    // 自定义场景的路径
    public override EventAssetProfile AssetProfile => new(BackgroundScenePath: "res://ArknightsMap/scenes/ancients/Lemuen.tscn");

    // 自定义地图图标和轮廓的路径
    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile =>
        new(
            MapIconPath: "res://ArknightsMap/images/ancients/Lemuen/icon.png",
            MapIconOutlinePath: "res://ArknightsMap/images/ancients/Lemuen/icon_outline.png",
            RunHistoryIconPath: "res://ArknightsMap/images/ancients/Lemuen/avatar.png",
            RunHistoryIconOutlinePath: "res://ArknightsMap/images/ancients/Lemuen/avatar.png"
        );

    private IReadOnlyList<EventOption> Pool1 =>
        [
<<<<<<< Updated upstream
            CreateModRelicOption<Internationalis>(), // 跨境追缉许可
            CreateModRelicOption<MementoMori>(), // 礼炮：强制追思
            
        ];
    private IReadOnlyList<EventOption> Pool2 =>
        [
            CreateModRelicOption<Hand>(), // 剿袭之手
            
=======
            CreateModRelicOption<CustomMade>(), // 定制铳械
            CreateModRelicOption<Solvent>(), // 铳械清洗溶剂
            CreateModRelicOption<WeiLaterano>(), // 盟约：拉特兰
        ];
    private IReadOnlyList<EventOption> Pool2 =>
        [
            CreateModRelicOption<GiantGun>(), // 铳骑的巨铳
            CreateModRelicOption<NoSugarIce>(), // 无糖冰淇淋
            CreateModRelicOption<Target>(), // 靶子
>>>>>>> Stashed changes
            
        ];
    private IReadOnlyList<EventOption> Pool3 =>
        [
<<<<<<< Updated upstream
            CreateModRelicOption<Wheelchair>(), // 轮椅
            
=======
            CreateModRelicOption<Aphasia>(), // “失语”
            CreateModRelicOption<Vannini>(), // 梵妮的铳
            CreateModRelicOption<Wrap>(), // 披肩
            CreateModRelicOption<ChildrenBook>(), 
>>>>>>> Stashed changes
            
        ];

    // 所有可能的选项
    public override IEnumerable<EventOption> AllPossibleOptions => [.. Pool1, .. Pool2, .. Pool3];

    // 生成选项
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return [Rng.NextItem(Pool1)!, Rng.NextItem(Pool2)!, Rng.NextItem(Pool3)!];
    }

    
}