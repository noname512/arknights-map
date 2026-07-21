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
public class Empathy : ModEnchantmentTemplate
{
    // 是否在卡牌上显示数值
    public override bool ShowAmount => false;

    // 是否会添加额外的卡牌描述文本
    public override bool HasExtraCardText => false;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        
    ];

    // 图标位置。大小1:1就行，原版是64x64
    public override EnchantmentAssetProfile AssetProfile => new(IconPath: $"res://ArknightsMap/images/enchantments/{GetType().Name}.png");

    private CardPlay? cardPlay = null;
    

    


    public override Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Status == MegaCrit.Sts2.Core.Entities.Enchantments.EnchantmentStatus.Normal && cardPlay.Card == this.Card)
        {
            foreach (CardModel c in base.Card.Owner.PlayerCombatState.AllCards.ToList())
            {
                if (c.Enchantment is Empathy && c.Pile.Type != PileType.Hand && c != this.Card)
                {
                    CardPileCmd.Add(c, PileType.Hand, CardPilePosition.Bottom);
                }

            }
            this.Status = MegaCrit.Sts2.Core.Entities.Enchantments.EnchantmentStatus.Disabled;
            
        }
        return base.AfterCardPlayedLate(choiceContext, cardPlay);
    }

    
}