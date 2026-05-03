using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Enchantments;

[RegisterEnchantment]
public class Flaming : ModEnchantmentTemplate
{
    // 是否在卡牌上显示数值
    public override bool ShowAmount => false;

    // 是否会添加额外的卡牌描述文本
    public override bool HasExtraCardText => true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<FlamingDamagePower>();

    // 像卡牌、遗物、药水等一样，可以使用DynamicVars和ExtraHoverTips
    // protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Retain)];

    // 图标位置。大小1:1就行，原版是64x64
    public override EnchantmentAssetProfile AssetProfile => new(
        IconPath: "res://icon.svg"
    );

    public override bool CanEnchantCardType(CardType cardType)
    {
        return cardType == CardType.Attack;
    }
    
    public override Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (cardSource == Card)
        {
            PowerCmd.Apply<FlamingDamagePower>(target, amount, dealer, Card);
        }
        return Task.CompletedTask;
    }
}
