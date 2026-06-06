using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Enchantments;

[RegisterEnchantment]
public class SpiralSpecial : ModEnchantmentTemplate
{
    // 是否在卡牌上显示数值
    public override bool ShowAmount => false;

    // 是否会添加额外的卡牌描述文本
    public override bool HasExtraCardText => false;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Times", 1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.ReplayDynamic, DynamicVars["Times"])];

    // 像卡牌、遗物、药水等一样，可以使用DynamicVars和ExtraHoverTips
    // protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Retain)];

    // 图标位置。大小1:1就行，原版是64x64
    public override EnchantmentAssetProfile AssetProfile => new(
        IconPath: "res://images/enchantments/spiral.png"
    );

    public override int EnchantPlayCount(int originalPlayCount)
    {
        return originalPlayCount + DynamicVars["Times"].IntValue;
    }
}
