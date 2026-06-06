
using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Relics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

[RegisterActEvent(typeof(Wilds))]
public sealed class TheWake : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: $"res://ArknightsMap/images/events/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(13m, ValueProp.Unblockable | ValueProp.Unpowered),
        new CardsVar(2),
        new HealVar(13),
        new EnergyVar(1)
    ];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, RemoveCards, InitialOptionKey("REMOVE_CARDS")),
        new EventOption(this, HealAndCardReward, InitialOptionKey("HEAL_AND_CARD_REWARD")),
        new EventOption(this, GainRelic, InitialOptionKey("GAIN_RELIC")),
    ];

    private async Task RemoveCards()
    {
        foreach (CardModel item in await CardSelectCmd.FromDeckForRemoval(prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, DynamicVars.Cards.IntValue), player: Owner!))
        {
            await CardPileCmd.RemoveFromDeck(item);
        }
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars.Damage, null, null);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.REMOVE_CARDS.description"));
    }

    private async Task HealAndCardReward()
    {
        await CreatureCmd.Heal(Owner!.Creature, DynamicVars.Heal.IntValue, true);
        CardCreationOptions options = CardCreationOptions.ForNonCombatWithUniformOdds([Owner!.Character.CardPool], c => c.Rarity == CardRarity.Uncommon).WithFlags(CardCreationFlags.NoRarityModification);
        await RewardsCmd.OfferCustom(Owner!, [new CardReward(options, 3, Owner)]);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.HEAL_AND_CARD_REWARD.description"));
    }

    private async Task GainRelic()
    {
        await RelicCmd.Obtain(ModelDb.Relic<EssenceInsight>().ToMutable(), Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.GAIN_RELIC.description"));
    }
}