
using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Cards;
using ArknightsMap.Scripts.Encounters;
using ArknightsMap.Scripts.Relics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Events;

[RegisterActEvent(typeof(Wilds))]
public sealed class OverlookingNasaoirsi : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: $"res://ArknightsMap/images/events/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("MaxHp1", 10),
        new IntVar("MaxHp2", 30),
        new IntVar("MaxHp3", 30),
        new IntVar("DamageIncrease", 40),
        new IntVar("IntangibleAmount", 1),
        new StringVar("CardType"),
        new StringVar("ExtraDescription")
    ];

    CardRarity maxRarity;

    protected override Task BeforeEventStarted(bool isPreFinished)
    {
        maxRarity = CardRarity.None;
        foreach (CardModel c in Owner!.Deck.Cards)
            if (c.Rarity <= CardRarity.Rare && c.Rarity > maxRarity)
                maxRarity = c.Rarity;
        StringVar stringVar = (StringVar)DynamicVars["CardType"];
        StringVar stringVar2 = (StringVar)DynamicVars["ExtraDescription"];
        LocString locString = L10NLookup($"{Id.Entry}.pages.INITIAL.options.SALVATION.extra_description");
        switch (maxRarity)
        {
            case CardRarity.Rare:
                stringVar.StringValue = new LocString("card_library", "RARITY_RARE").GetRawText();
                locString.Add("MaxHp4", 20);
                stringVar2.StringValue = locString.GetFormattedText();
                break;
            case CardRarity.Uncommon:
                stringVar.StringValue = new LocString("card_library", "RARITY_UNCOMMON").GetRawText();
                locString.Add("MaxHp4", 10);
                stringVar2.StringValue = locString.GetFormattedText();
                break;
            case CardRarity.Common:
                stringVar.StringValue = new LocString("card_library", "RARITY_COMMON").GetRawText();
                stringVar2.StringValue = "";
                break;
            default:
                break;
        }
        return Task.CompletedTask;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> list = [];
        switch (Owner!.RunState.Act.BossEncounter)
        {
            case MandragoraBoss:
                list.Add(new EventOption(this, Vengeance, InitialOptionKey("VENGEANCE")));
                break;
            case HerFlame:
                list.Add(new EventOption(this, Rebirth, InitialOptionKey("REBIRTH")));
                break;
            case AFRBoss:
                list.Add(new EventOption(this, Incinerate, InitialOptionKey("INCINERATE")));
                break;
        }
        if (maxRarity >= CardRarity.Common) list.Add(new EventOption(this, Salvation, InitialOptionKey("SALVATION")));
        list.Add(new EventOption(this, Proliferation, InitialOptionKey("PROLIFERATION")));
        return list;
    }

    private async Task Vengeance()
    {
        await CreatureCmd.LoseMaxHp(new BlockingPlayerChoiceContext(), Owner!.Creature, DynamicVars["MaxHp1"].IntValue, false);
        await RelicCmd.Obtain<Vengeance>(Owner);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.VENGEANCE.description"));
    }

    private async Task Rebirth()
    {
        await CreatureCmd.LoseMaxHp(new BlockingPlayerChoiceContext(), Owner!.Creature, DynamicVars["MaxHp2"].IntValue, false);
        await RelicCmd.Obtain<Rebirth>(Owner);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.REBIRTH.description"));
    }

    private async Task Incinerate()
    {
        await CreatureCmd.LoseMaxHp(new BlockingPlayerChoiceContext(), Owner!.Creature, DynamicVars["MaxHp3"].IntValue, false);
        await RelicCmd.Obtain<Incinerate>(Owner);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.INCINERATE.description"));
    }

    private async Task Salvation()
    {
        foreach (CardModel item in await CardSelectCmd.FromDeckForRemoval(
            Owner!,
            new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 1) { RequireManualConfirmation = true },
            c => c.Rarity == maxRarity
        ))
        {
            await CardPileCmd.RemoveFromDeck(item);
        }
        if (maxRarity == CardRarity.Rare) await CreatureCmd.GainMaxHp(Owner!.Creature, 20);
        else if (maxRarity == CardRarity.Uncommon) await CreatureCmd.GainMaxHp(Owner!.Creature, 10);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.SALVATION.description"));
    }

    private async Task Proliferation()
    {
        CardModel card = Owner!.RunState.CreateCard<Proliferation>(Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.PROLIFERATION.description"));
    }
}