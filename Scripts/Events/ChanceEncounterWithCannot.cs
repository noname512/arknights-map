using ArknightsMap.Scripts.Acts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Events;

[RegisterActEvent(typeof(Wilds))]
public sealed class ChanceEncounterWithCannot : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(InitialPortraitPath: $"res://ArknightsMap/images/events/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [
            new IntVar("gold1", 0),
            new StringVar("Relic"),
            new IntVar("gold2", 0),
            new StringVar("Card"),
            new IntVar("gold3", 0),
            new StringVar("Potion"),
            new StringVar("Card2"),
            new DamageVar(10, ValueProp.Unblockable | ValueProp.Unpowered),
        ];

    RelicModel? relic;
    PotionModel? potion;
    CardModel? card;
    CardModel? card2;

    protected override Task BeforeEventStarted(bool isPreFinished)
    {
        Owner!.CanRemovePotions = false;
        return Task.CompletedTask;
    }

    protected override void OnEventFinished()
    {
        Owner!.CanRemovePotions = true;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> options = [];
        if (Owner!.Relics.Count > 0)
        {
            relic = Rng.NextItem(Owner.Relics)!;
            StringVar stringVar = (StringVar)DynamicVars["Relic"];
            stringVar.StringValue = relic.Title.GetRawText();
            DynamicVars["gold1"].BaseValue = Rng.NextInt(130, 170);
            options.Add(new EventOption(this, Relic, InitialOptionKey("RELIC"), HoverTipFactory.FromRelic(relic)));
        }
        else
        {
            options.Add(new EventOption(this, null, InitialOptionKey("RELIC_LOCKED")));
        }
        if (Owner!.Potions.Count() > 0)
        {
            potion = Rng.NextItem(Owner.Potions)!;
            StringVar stringVar = (StringVar)DynamicVars["Potion"];
            stringVar.StringValue = potion.Title.GetRawText();
            DynamicVars["gold2"].BaseValue = Rng.NextInt(90, 110);
            options.Add(new EventOption(this, Potion, InitialOptionKey("POTION"), HoverTipFactory.FromPotion(potion)));
        }
        else
        {
            options.Add(new EventOption(this, null, InitialOptionKey("POTION_LOCKED")));
        }
        List<CardModel> list = Owner.Deck.Cards.Where(c => c.IsRemovable && c.Rarity is not CardRarity.Basic).ToList();
        if (list.Count() > 0)
        {
            card = Rng.NextItem(list)!;
            StringVar stringVar = (StringVar)DynamicVars["Card"];
            stringVar.StringValue = card.Title;
            DynamicVars["gold3"].BaseValue = Rng.NextInt(60, 75);
            options.Add(new EventOption(this, Card, InitialOptionKey("CARD"), HoverTipFactory.FromCard(card)));
        }
        else
        {
            options.Add(new EventOption(this, null, InitialOptionKey("CARD_LOCKED")));
        }
        list = Owner.Deck.Cards.Where(c => c.Rarity is CardRarity.Basic && (c.Tags.Contains(CardTag.Strike) || c.Tags.Contains(CardTag.Defend))).ToList();
        if (list.Count() > 0)
        {
            card2 = Rng.NextItem(list)!;
            StringVar stringVar = (StringVar)DynamicVars["Card2"];
            stringVar.StringValue = card2.Title;
            options.Add(new EventOption(this, StrikeDefend, InitialOptionKey("STRIKE_DEFEND"), HoverTipFactory.FromCard(card2)));
        }
        else
        {
            options.Add(new EventOption(this, null, InitialOptionKey("STRIKE_DEFEND_LOCKED")));
        }
        return options;
    }

    private async Task Relic()
    {
        await RelicCmd.Remove(relic!);
        await PlayerCmd.GainGold(DynamicVars["gold1"].IntValue, Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.TRADE.description"));
    }

    private async Task Potion()
    {
        await PotionCmd.Discard(potion!);
        await PlayerCmd.GainGold(DynamicVars["gold2"].IntValue, Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.TRADE.description"));
    }

    private async Task Card()
    {
        await CardPileCmd.RemoveFromDeck(card!);
        await PlayerCmd.GainGold(DynamicVars["gold3"].IntValue, Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.TRADE.description"));
    }

    private async Task StrikeDefend()
    {
        await CardPileCmd.RemoveFromDeck(card!);
        await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), Owner!.Creature, DynamicVars.Damage, null, null);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.FAILURE.description"));
    }
}
