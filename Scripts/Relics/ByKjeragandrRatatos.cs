using ArknightsMap.Scripts.Utils.MerchantEnchantment;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class ByKjeragandrRatatos : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public override bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewards, CardCreationOptions options)
    {
        if (player != Owner)
        {
            return false;
        }
        if (!options.Flags.HasFlag(CardCreationFlags.IsCardReward))
        {
            return false;
        }
        foreach (CardCreationResult cardReward in cardRewards)
        {
            CardModel card = cardReward.Card;
            EnchantmentModel enchantment = EnchantmentMerchantUtils.GenerateModel(card, Owner.PlayerRng.Rewards);
            CardModel card2 = Owner.RunState.CloneCard(card);
            CardCmd.Enchant(enchantment, card2, enchantment.Amount);
            cardReward.ModifyCard(card2, this);
        }
        return true;
    }
}
