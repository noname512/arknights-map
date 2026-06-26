using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Enchantments;

[RegisterEnchantment]
public class BlockHeal : ModEnchantmentTemplate
{
    // 是否在卡牌上显示数值
    public override bool ShowAmount => false;

    // 是否会添加额外的卡牌描述文本
    public override bool HasExtraCardText => true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockHealDynamicVar(0, ValueProp.Move)
    ];

    // 图标位置。大小1:1就行，原版是64x64
    public override EnchantmentAssetProfile AssetProfile => new(IconPath: $"res://ArknightsMap/images/enchantments/{GetType().Name}.png");

    private CardPlay? cardPlay = null;
    public override bool CanEnchantCardType(CardType cardType)
    {
        return cardType == CardType.Attack;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        this.cardPlay = cardPlay;
        return Task.CompletedTask;
    }

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource
    )
    {
        if (cardSource == Card)
        {
            BlockVar var = new BlockVar(amount / 2, ValueProp.Move);
            await CreatureCmd.GainBlock(Card.Owner.Creature, var, cardPlay);
        }
    }
}
