using ArknightsMap.Scripts.Acts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Events;

[RegisterActEvent(typeof(Wilds))]
public sealed class HaystackMidnightTalks : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(InitialPortraitPath: $"res://ArknightsMap/images/events/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> list = [];
        if (PossibleChange(Owner))
        {
            list.Add(new EventOption(this, MakeAChange, InitialOptionKey("MAKE_A_CHANGE")));
        }
        else
        {
            list.Add(new EventOption(this, null, InitialOptionKey("MAKE_A_CHANGE_LOCK")));
        }
        if (PossibleBury(Owner))
        {
            list.Add(new EventOption(this, BuryTheTruth, InitialOptionKey("BURY_THE_TRUTH")));
        }
        else
        {
            list.Add(new EventOption(this, null, InitialOptionKey("BURY_THE_TRUTH_LOCK")));
        }
        return list;
    }

    public override bool IsAllowed(IRunState runState)
    {
        return runState.Players.All(p => PossibleChange(p) || PossibleBury(p));
    }

    public bool PossibleChange(Player player)
    {
        return player.Deck.Cards.Any(c => c.IsTransformable);
    }

    public bool PossibleBury(Player player)
    {
        return player.Deck.Cards.Count(c => c.IsRemovable) >= 2;
    }

    private async Task MakeAChange()
    {
        CardModel cardModel = (
            await CardSelectCmd.FromDeckForTransformation(Owner!, new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1))
        ).FirstOrDefault()!;
        if (cardModel != null)
        {
            CardModel newCard = CardFactory.CreateRandomCardForTransform(cardModel, isInCombat: false, Rng);
            CardCmd.Upgrade(newCard);
            await CardCmd.Transform(cardModel, newCard);
        }
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.MAKE_A_CHANGE.description"));
    }

    private async Task BuryTheTruth()
    {
        foreach (CardModel item in await CardSelectCmd.FromDeckForRemoval(Owner!, new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, 2)))
        {
            await CardPileCmd.RemoveFromDeck(item);
        }
        await CardPileCmd.AddCurseToDeck<Guilty>(Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.BURY_THE_TRUTH.description"));
    }
}
