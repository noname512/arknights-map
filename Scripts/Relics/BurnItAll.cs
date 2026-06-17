using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class BurnItAll : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(55), new IntVar("CardsPick", 25)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromCard<Dirge>()];

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
        IEnumerable<CardModel> cardsPile = PileType.Deck.GetPile(Owner).Cards.ToList();
        bool hasDirge = false;
        foreach (CardModel item in cardsPile)
        {
            if (item is Dirge)
            {
                hasDirge = true;
            }
        }

        PileType.Deck.GetPile(Owner).Clear();

        Player player = Owner;

        CardCreationOptions options = CardCreationOptions
            .ForNonCombatWithUniformOdds(
                [Owner.Character.CardPool],
                c => c.Rarity != CardRarity.Basic && c.Rarity != CardRarity.Event && c.Rarity != CardRarity.Ancient
            )
            .WithFlags(CardCreationFlags.NoRarityModification);
        List<CardCreationResult> cards = [];
        if (hasDirge)
        {
            CardCreationOptions rareOptions = CardCreationOptions
                .ForNonCombatWithUniformOdds([Owner.Character.CardPool], c => c.Rarity == CardRarity.Rare)
                .WithFlags(CardCreationFlags.NoRarityModification);
            cards.AddRange(CardFactory.CreateForReward(player, rareOptions.GetPossibleCards(player).Count(), rareOptions));
        }

        while (cards.Count < DynamicVars.Cards.IntValue)
        {
            cards.AddRange(CardFactory.CreateForReward(player, 1, options));
        }

        cards.Sort((a, b) => a.Card.Rarity - b.Card.Rarity);

        foreach (
            CardModel item in await CardSelectCmd.FromSimpleGridForRewards(
                prefs: new CardSelectorPrefs(L10NLookup("ARKNIGHTS_MAP_RELIC_HER_ALLOWANCE.choose"), DynamicVars["CardsPick"].IntValue),
                context: new BlockingPlayerChoiceContext(),
                cards: cards,
                player: Owner
            )
        )
        {
            CardModel newCard = Owner.RunState.CreateCard(item, Owner);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(newCard, PileType.Deck));
        }
    }
}
