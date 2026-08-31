using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Relics;
using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Ancients;

[RegisterSharedAncient]
public class Executor : ModAncientEventTemplate
{
    // 选项按钮颜色
    public override Color ButtonColor => new(0.12f, 0.2f, 0.8f, 0.5f);

    // 对话框颜色
    public override Color DialogueColor => new(0.12f, 0.2f, 0.8f);

    // 自定义场景的路径
    public override EventAssetProfile AssetProfile => new(BackgroundScenePath: "res://ArknightsMap/scenes/ancients/Executor.tscn");

    // 自定义地图图标和轮廓的路径
    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile =>
        new(
            MapIconPath: "res://ArknightsMap/images/ancients/Executor/icon.png",
            MapIconOutlinePath: "res://ArknightsMap/images/ancients/Executor/icon_outline.png",
            RunHistoryIconPath: "res://ArknightsMap/images/ancients/Executor/avatar.png",
            RunHistoryIconOutlinePath: "res://ArknightsMap/images/ancients/Executor/avatar.png"
        );

    private IReadOnlyList<EventOption> Pool1 =>
        [
            CreateModRelicOption<ExFoedere>(), // 圣约
            CreateModRelicOption<Shotgun>(), // 近身铳斗
            CreateModRelicOption<SaintMind>(), // 圣徒意志
            CreateModRelicOption<Lens>(), // 精密瞄准镜
            
        ];
    private IReadOnlyList<EventOption> Pool2 =>
        [
            CreateModRelicOption<PreciseMachine>(), // 精密仪器
        ];
    private IReadOnlyList<EventOption> Pool3 =>
        [
            CreateModRelicOption<UnAnswered>(), // 未解答
            CreateModRelicOption<WarnBullet>(), // 示警铳弹
        ];

    // 所有可能的选项
    public override IEnumerable<EventOption> AllPossibleOptions => [.. Pool1, .. Pool2, .. Pool3];

    // 生成选项
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return [Rng.NextItem(Pool1)!, Rng.NextItem(Pool2)!, Rng.NextItem(Pool3)!];
    }

    public override bool IsValidForAct(ActModel act)
    {
        return act is Laterano;
    }
}
