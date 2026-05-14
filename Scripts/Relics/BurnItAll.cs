using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArknightsMap.Scripts.Enchantments;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class BurnItAll : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(55), new IntVar("CardsPick",20)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromCard<Dirge>()];

    public override RelicAssetProfile AssetProfile => new(
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
        
        /*

        Player player = Owner;
        
        List<CardModel> possibleCards = Owner.Character.CardPool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint).Where((c => c.Rarity != CardRarity.Basic && c.Rarity != CardRarity.Ancient && c.Rarity != CardRarity.Event)).ToList();
        List<CardModel> chosenCards = [];
        List<CardCreationResult> cards = [];
        if (hasDirge)
        {
            chosenCards.AddRange(possibleCards.ToArray().Where(c => c.Rarity == CardRarity.Rare).ToList());
        }

        while (chosenCards.Count <= DynamicVars.Cards.IntValue)
        {
            chosenCards.Add(possibleCards.ElementAt(0));
        }

        foreach (var card in chosenCards)
        {
            cards.Add(new CardCreationResult(card));
        }
        CardCreationOptions options = CardCreationOptions.ForNonCombatWithUniformOdds([Owner.Character.CardPool], c => c.Rarity != CardRarity.Basic && c.Rarity != CardRarity.Event && c.Rarity != CardRarity.Ancient).WithFlags(CardCreationFlags.NoRarityModification);

        if (Hook.TryModifyCardRewardOptions(player.RunState, player, cards, options, out List<AbstractModel> modifiers))
        {
            await TaskHelper.RunSafely(Hook.AfterModifyingCardRewardOptions(player.RunState, modifiers));
        }
        
        foreach (CardModel item in await CardSelectCmd.FromSimpleGridForRewards(prefs: new CardSelectorPrefs(L10NLookup("ROOM_FULL_OF_CHEESE.pages.GORGE.selectionScreenPrompt"), 20), context: new BlockingPlayerChoiceContext(), cards: cards, player: Owner))
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(item, PileType.Deck));
        }
        */
    }
}
