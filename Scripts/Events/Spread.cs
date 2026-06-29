using ArknightsMap.Scripts.Cards;
using ArknightsMap.Scripts.Encounters;
using ArknightsMap.Scripts.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Events;

[RegisterSharedEvent]
public sealed class Spread : ModEventTemplate
{
    public override bool IsShared => true;
    public override EventAssetProfile AssetProfile => new(InitialPortraitPath: $"res://ArknightsMap/images/events/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override bool IsAllowed(IRunState runState)
    {
        if (runState.Act.Index != 2)
        {
            return false;
        }
        foreach (Player player in runState.Players)
        {
            if (player.Deck.Cards.Any(c => c is Benediction))
            {
                return true;
            }
        }
        return false;
    }

    protected override Task BeforeEventStarted(bool isPreFinished)
    {
        foreach (Player player in Owner!.RunState.Players)
        {
            foreach (CardModel card in player.Deck.Cards.Where(c => c is Benediction).ToList())
            {
                CardPileCmd.RemoveFromDeck(card, player == Owner);
            }
        }
        return Task.CompletedTask;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
        [
            new EventOption(this, FaceTheCrowd, InitialOptionKey("FACE_THE_CROWD")),
            new EventOption(this, FaceTheSource, InitialOptionKey("FACE_THE_SOURCE")),
            new EventOption(this, AllowToRunWild, InitialOptionKey("ALLOW_TO_RUN_WILD")).WithRelic<Proliferate>(Owner),
        ];

    private async Task FaceTheCrowd()
    {
        List<Reward> list = new List<Reward>();
        foreach (Player player in Owner!.RunState.Players)
        {
            for (int i = 0; i < 2; i++)
            {
                list.Add(new RelicReward(player));
            }
        }
        EnterCombatWithoutExitingEvent<DublinnFlamechasers>(list, shouldResumeAfterCombat: false);
    }

    private async Task FaceTheSource()
    {
        List<Reward> list = new List<Reward>();
        foreach (Player player in Owner!.RunState.Players)
        {
            list.Add(new RelicReward(player));
        }
        EnterCombatWithoutExitingEvent<EndPointEncounter>(list, shouldResumeAfterCombat: false);
    }

    private async Task AllowToRunWild()
    {
        await RelicCmd.Obtain<Proliferate>(Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.ALLOW_TO_RUN_WILD.description"));
    }
}
