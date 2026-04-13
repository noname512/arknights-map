using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace ArknightsMap.Scripts.Ancients;

public class Reed : CustomAncientModel
{
    // 选项按钮颜色
    public override Color ButtonColor => new(0.12f, 0.2f, 0.8f, 0.5f);
    // 对话框颜色
    public override Color DialogueColor => new(0.12f, 0.2f, 0.8f);

    // 出现条件。这里是只能在第二幕出现
    public override bool IsValidForAct(ActModel act) => act.ActNumber() == 2;
    // 自定义场景的路径
    public override string? CustomScenePath => "res://ArknightsMap/scenes/Reed.tscn";
    // 自定义地图图标和轮廓的路径
    public override string? CustomMapIconPath => "res://icon.svg";
    public override string? CustomMapIconOutlinePath => "res://icon.svg";
    // 历史记录图标路径
    public override string? CustomRunHistoryIconPath => "res://icon.svg";
    public override string? CustomRunHistoryIconOutlinePath => "res://icon.svg";

    // 生成选项。每个选项有自己的池子。
    protected override OptionPools MakeOptionPools { get; } = new OptionPools(
        MakePool(
            AncientOption<Akabeko>(),
            AncientOption<Anchor>()
        ),
        MakePool(
            AncientOption<LizardTail>(),
            AncientOption<ArcaneScroll>()
        ),
        MakePool(
            AncientOption<YummyCookie>(weight: 2), // 加权重，权重越高越容易取到
            AncientOption<WingCharm>()
        )
    );
}