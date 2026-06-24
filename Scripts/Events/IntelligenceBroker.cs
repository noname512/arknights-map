using ArknightsMap.Scripts.Acts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Events;

[RegisterActEvent(typeof(Wilds))]
public sealed class IntelligenceBroker : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(InitialPortraitPath: $"res://ArknightsMap/images/events/{GetType().Name}.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new IntVar("gold1", 30), new IntVar("gold2", 40), new IntVar("gold3", 50), new StringVar("Info")];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
        [
            new EventOption(this, Thirty, InitialOptionKey("THIRTY")),
            new EventOption(this, Fourty, InitialOptionKey("FOURTY")),
            new EventOption(this, Fifty, InitialOptionKey("FIFTY")),
            new EventOption(this, Leave, InitialOptionKey("LEAVE")),
        ];

    public override bool IsAllowed(IRunState runState)
    {
        return runState.Act is Wilds && runState.Players.All(player => player.Gold >= 100);
    }

    private async Task Thirty()
    {
        await PlayerCmd.LoseGold(DynamicVars["gold1"].IntValue, Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.THIRTY.description"));
    }

    private async Task Fourty()
    {
        await PlayerCmd.LoseGold(DynamicVars["gold2"].IntValue, Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.FOURTY.description"));
    }

    private async Task Fifty()
    {
        await PlayerCmd.LoseGold(DynamicVars["gold3"].IntValue, Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.FIFTY.description"));
    }

    private Task Leave()
    {
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.LEAVE.description"));
        return Task.CompletedTask;
    }

    private async Task SaveLoad()
    {
        await PlayerCmd.LoseGold(DynamicVars["gold3"].IntValue, Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.SAVE_LOAD.description"));
    }
}
