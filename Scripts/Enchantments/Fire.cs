using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Enchantments;

[RegisterEnchantment]
public class Fire : ModEnchantmentTemplate
{
    // 是否在卡牌上显示数值
    public override bool ShowAmount => false;

    // 是否会添加额外的卡牌描述文本
    public override bool HasExtraCardText => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];

    // 图标位置。大小1:1就行，原版是64x64
    public override EnchantmentAssetProfile AssetProfile => new(IconPath: $"res://ArknightsMap/images/enchantments/{GetType().Name}.png");

    public override bool CanEnchant(CardModel card)
    {
        if (card.TargetType != TargetType.AnyEnemy)
        {
            return false;
        }
        return base.CanEnchant(card);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card != Card)
        {
            return;
        }
        List<CardModel> cards = CardFactory
            .GetForCombat(
                Card.Owner,
                from c in Card.Owner.Character.CardPool.AllCards
                where c.Type == CardType.Attack
                select c,
                2,
                Card.Owner.RunState.Rng.CombatCardGeneration
            )
            .ToList();
        foreach (CardModel card in cards)
        {
            await CardCmd.AutoPlay(choiceContext, card.CreateDupe(Card.Owner), cardPlay.Target);
        }
    }
}
