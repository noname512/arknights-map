
using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Ancients;
using ArknightsMap.Scripts.Encounters;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Events;

[RegisterSharedEvent]
public sealed class TheLeaderOfDublinn : ModEventTemplate
{
    public override bool IsShared => true;
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: $"res://ArknightsMap/images/events/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("VouivreName", L10NLookup($"{Id.Entry}.pages.INITIAL.unknownVouivre").GetRawText()),
        new StringVar("DracoName", L10NLookup($"{Id.Entry}.pages.INITIAL.unknownDraco").GetRawText()),
        new DamageVar(20, ValueProp.Unpowered | ValueProp.Unblockable),
        new BlockVar(15, ValueProp.Unpowered),
        new EnergyVar(3),
        new CardsVar(3)
    ];

    private bool isBagpipe = false;
    private bool isReed = false;

    public override bool IsAllowed(IRunState runState)
    {
        return runState.Act is Wilds;
    }

    protected override Task BeforeEventStarted(bool isPreFinished)
    {
        foreach (var item in Owner!.RunState.MapPointHistory)
            foreach (var item2 in item)
                foreach (MapPointRoomHistoryEntry room in item2.Rooms)
                    if (room.RoomType == RoomType.Event)
                    {
                        if (room.ModelId == ModelDb.AncientEvent<Bagpipe>().Id)
                        {
                            StringVar stringVar = (StringVar)DynamicVars["VouivreName"];
                            stringVar.StringValue = L10NLookup($"{Id.Entry}.pages.INITIAL.knownVouivre").GetRawText();
                            isBagpipe = true;
                        }
                        else if (room.ModelId == ModelDb.AncientEvent<Reed>().Id)
                        {
                            StringVar stringVar = (StringVar)DynamicVars["DracoName"];
                            stringVar.StringValue = L10NLookup($"{Id.Entry}.pages.INITIAL.knownDraco").GetRawText();
                            isReed = true;
                        }
                    }
        return Task.CompletedTask;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> options = new List<EventOption>();
        if (isBagpipe) options.Add(new EventOption(this, ExtractTheTruth, InitialOptionKey("EXTRACT_THE_TRUTH")));
        else if (isReed) options.Add(new EventOption(this, FaceTheAnger, InitialOptionKey("FACE_THE_ANGER")));
        else options.Add(new EventOption(this, LackOfAbility, InitialOptionKey("LACK_OF_ABILITY")));
        options.Add(new EventOption(this, FindCommonGround, InitialOptionKey("FIND_COMMON_GROUND")));
        return options;
    }

    private async Task ExtractTheTruth()
    {
        await SelectCardAndAdd(c => c.Type == CardType.Attack && c.DynamicVars.ContainsKey("Damage") && c.DynamicVars.Damage.BaseValue >= DynamicVars.Damage.BaseValue);
    }

    private async Task FaceTheAnger()
    {
        await SelectCardAndAdd(c => c.DynamicVars.ContainsKey("Block") && c.DynamicVars.Block.BaseValue >= DynamicVars.Block.BaseValue);
    }

    private async Task LackOfAbility()
    {
        await SelectCardAndAdd(c => c.Type == CardType.Power && c.EnergyCost.Canonical == DynamicVars.Energy.BaseValue);
    }

    private async Task SelectCardAndAdd(Func<CardModel, bool> filter)
    {
        CardCreationOptions options = CardCreationOptions.ForNonCombatWithDefaultOdds(ModelDb.AllCharacterCardPools, filter);
        List<CardCreationResult> cards = CardFactory.CreateForReward(Owner!, DynamicVars.Cards.IntValue, options).ToList();
        foreach (var item in await CardSelectCmd.FromSimpleGridForRewards(new BlockingPlayerChoiceContext(), cards, Owner!, new CardSelectorPrefs(L10NLookup($"{Id.Entry}.pages.INITIAL.selectionScreenPrompt"), 1)))
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(item, PileType.Deck));
        }
        if (!isReed)
        {
            StringVar stringVar = (StringVar)DynamicVars["DracoName"];
            stringVar.StringValue = L10NLookup($"{Id.Entry}.pages.INITIAL.unknownDraco2").GetRawText();
        }
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.END.description"));
    }

    private async Task FindCommonGround()
    {
        EnterCombatWithoutExitingEvent<TheRedsteelGuard>(Array.Empty<Reward>(), shouldResumeAfterCombat: false);
    }
}