using System.Reflection;
using ArknightsMap.Scripts.Acts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Events;

[RegisterActEvent(typeof(Wilds))]
public sealed class IntelligenceBroker : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(InitialPortraitPath: $"res://ArknightsMap/images/events/{GetType().Name}.png");

    private static RelicRarity[] rarities = [RelicRarity.Common, RelicRarity.Uncommon, RelicRarity.Rare];

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            List<DynamicVar> list = [new IntVar("gold1", 30), new IntVar("gold2", 40), new IntVar("gold3", 50), new IntVar("chosen", 0)];
            for (int i = 1; i <= 3; i++)
            {
                list.Add(new StringVar("relic" + i, ""));
                foreach (RelicRarity rarity in rarities)
                {
                    list.Add(new StringVar("relic" + rarity.ToString() + i, ""));
                }
            }
            return list;
        }
    }

    [SavedProperty]
    public static int AlreadyChoose = 0;

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> options =
        [
            new EventOption(this, Thirty, InitialOptionKey("THIRTY")),
            new EventOption(this, Fourty, InitialOptionKey("FOURTY")),
            new EventOption(this, Fifty, InitialOptionKey("FIFTY")),
            new EventOption(this, Leave, InitialOptionKey("LEAVE")),
        ];
        if (AlreadyChoose != 0)
        {
            for (int i = 0; i < 4; i++)
                if (i != AlreadyChoose - 1)
                    options[i] = new EventOption(this, null, InitialOptionKey("LOCKED"));
            for (int i = 1; i <= 3; i++)
                DynamicVars["gold" + i].BaseValue *= 2;
            DynamicVars["chosen"].BaseValue = 1;
        }
        return options;
    }

    public override bool IsAllowed(IRunState runState)
    {
        return runState.Act is Wilds && runState.Players.All(player => player.Gold >= 100);
    }

    private async Task Thirty()
    {
        AlreadyChoose = 1;
        await PlayerCmd.LoseGold(DynamicVars["gold1"].IntValue, Owner!);
        var method = typeof(RelicGrabBag).GetMethod("GetAvailableDeque", BindingFlags.Instance | BindingFlags.NonPublic)!;
        object? obj = method.Invoke(Owner!.RelicGrabBag, [RelicRarity.Shop, Owner!.RunState, (RelicModel _) => true]);
        if (obj != null)
        {
            List<RelicModel> availableDeque = (List<RelicModel>)obj;
            int start = Math.Min(availableDeque.Count, 3);
            for (int i = start - 1; i >= 0; i--)
            {
                RelicModel relic = availableDeque[i];
                StringVar stringVar = (StringVar)DynamicVars["relic" + (start - i)];
                stringVar.StringValue = relic.Title.GetRawText();
            }
            SetEventFinished(L10NLookup($"{Id.Entry}.pages.THIRTY.description"));
        }
        else
        {
            SetEventFinished(L10NLookup($"{Id.Entry}.pages.THIRTY.description_empty"));
        }
    }

    private async Task Fourty()
    {
        AlreadyChoose = 2;
        await PlayerCmd.LoseGold(DynamicVars["gold2"].IntValue, Owner!);
        var method = typeof(RelicGrabBag).GetMethod("GetAvailableDeque", BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (RelicRarity rarity in rarities)
        {
            object? obj = method.Invoke(Owner!.RelicGrabBag, [rarity, Owner!.RunState, (RelicModel _) => true]);
            if (obj == null)
            {
                continue;
            }
            List<RelicModel> availableDeque = (List<RelicModel>)obj;
            for (int i = 0; i < Math.Min(availableDeque.Count, 3); i++)
            {
                RelicModel relic = availableDeque[i];
                StringVar stringVar = (StringVar)DynamicVars["relic" + rarity.ToString() + (i + 1)];
                stringVar.StringValue = relic.Title.GetRawText();
            }
        }
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.FOURTY.description"));
    }

    private async Task Fifty()
    {
        AlreadyChoose = 3;
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
