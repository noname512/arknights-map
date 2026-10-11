using System.Reflection;
using ArknightsMap.Scripts.Cards;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class AncientTome : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public override async Task AfterObtained()
    {
        List<Reward> rewards = [];
        for (int i = 0; i < DynamicVars.Cards.BaseValue; i++)
        {
            CardCreationOptions options = new CardCreationOptions(
                [ModelDb.CardPool<EventCardPool>()],
                CardCreationSource.Other,
                (CardRarityOddsType)325,
                c => c.Rarity == CardRarity.Ancient && c is not SomethingForm
            ).WithFlags(CardCreationFlags.NoRarityModification | CardCreationFlags.NoCardPoolModifications | CardCreationFlags.NoCardModelModifications);
            rewards.Add(new CardReward(options, 3, Owner));
        }
        await RewardsCmd.OfferCustom(Owner, rewards);
    }

    [HarmonyPatch]
    public static class CreateForRewardPatch
    {
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(CardFactory), "CreateForReward", [typeof(Player), typeof(IEnumerable<CardModel>), typeof(CardCreationOptions)]);
        }

        static bool Prefix(Player player, IEnumerable<CardModel> blacklist, CardCreationOptions options, ref CardModel __result)
        {
            if (options.RarityOdds == (CardRarityOddsType)325)
            {
                options = Hook.ModifyCardRewardCreationOptions(player.RunState, player, options);
                IEnumerable<CardModel> items = options.GetPossibleCards(player).Except(blacklist).ToList();
                var method = AccessTools.Method(typeof(CardFactory), "FilterForPlayerCount", [typeof(IRunState), typeof(IEnumerable<CardModel>)]);
                items = ((IEnumerable<CardModel>)method.Invoke(null, [player.RunState, items])!).ToArray();
                Rng rng = options.RngOverride ?? player.PlayerRng.Rewards;
                CardModel? cardModel = rng.NextItem(items);
                if (cardModel == null)
                {
                    throw new InvalidOperationException(
                        $"Tried to create a card for a reward, but we couldn't generate a valid card! card pool: {string.Join(",", items)}, blacklist: {string.Join(",", blacklist)}, odds: {options.RarityOdds}"
                    );
                }
                __result = player.RunState.CreateCard(cardModel, player);
                return false;
            }
            return true;
        }
    }
}
