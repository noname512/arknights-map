
using Godot;
using ArknightsMap.Scripts.Ancients;
using ArknightsMap.Scripts.Encounters;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Unlocks;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Random;
using STS2RitsuLib.Interop.AutoRegistration;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Encounters;

namespace ArknightsMap.Scripts.Acts;

[RegisterAct]
public sealed class Wilds : ModActTemplate
{
    public override string[] MusicBankPaths => new string[2] { "res://banks/desktop/act2_a1.bank", "res://banks/desktop/act2_a2.bank" };
    public override string[] BgMusicOptions => new string[2] { "event:/music/act2_a1_v2", "event:/music/act2_a2_v2" };

    public override Color MapTraveledColor => new Color("27221C");

    public override Color MapUntraveledColor => new Color("6E7750");

    public override Color MapBgColor => new Color("9B9562");

    public override string ChestSpineResourcePath => "res://animations/backgrounds/treasure_room/chest_room_act_2_skel_data.tres";
    public override string ChestSpineSkinNameNormal => "act2";
    public override string ChestSpineSkinNameStroke => "act2_stroke";
    public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act2";
    public override string AmbientSfx => "event:/sfx/ambience/act2_ambience";
    protected override int BaseNumberOfRooms => 14;

    public override string? CustomBackgroundScenePath => "res://ArknightsMap/scenes/acts/Wilds/wilds_background.tscn";
    public override string? CustomBackgroundLayersDirectoryPath => "res://ArknightsMap/scenes/acts/Wilds/layers";
    public override string? CustomMapTopBgPath => "res://images/packed/map/map_bgs/hive/map_top_hive.png";
    public override string? CustomMapMidBgPath => "res://images/packed/map/map_bgs/hive/map_middle_hive.png";
    public override string? CustomMapBotBgPath => "res://images/packed/map/map_bgs/hive/map_bottom_hive.png";
    public override string? CustomRestSiteBackgroundPath => "res://scenes/rest_site/hive_rest_site.tscn";

    public override IEnumerable<EventModel> AllEvents => new EventModel[]
    {
        ModelDb.Event<Amalgamator>(),
    };

    public override IEnumerable<AncientEventModel> AllAncients => new AncientEventModel[]
    {
        ModelDb.AncientEvent<Reed>(),
        ModelDb.AncientEvent<Bagpipe>(),
    };

    public override IEnumerable<EncounterModel> BossDiscoveryOrder => new EncounterModel[]
    {
        ModelDb.Encounter<TheInsatiableBoss>()
    };

    public override IEnumerable<EncounterModel> GenerateAllEncounters() => new EncounterModel[]
    {
        ModelDb.Encounter<BurnTheHaystacks>(),
        ModelDb.Encounter<DublinnPhalanx>(),
        ModelDb.Encounter<PatrollingFoliageNormal>(),
        ModelDb.Encounter<DublinnCompanion>(),
    };

    protected override void ApplyActDiscoveryOrderModifications(UnlockState unlockState) { }

    public override IEnumerable<AncientEventModel> GetUnlockedAncients(UnlockState unlockState)
    {
        List<AncientEventModel> list = AllAncients.ToList();
        // if (!unlockState.IsEpochRevealed<OrobasEpoch>())
        // {
        //     list.Remove(ModelDb.AncientEvent<Orobas>());
        // }
        return list;
    }

    public override MapPointTypeCounts GetMapPointTypes(Rng mapRng)
    {
        int restCount = mapRng.NextGaussianInt(6, 1, 6, 7);
        int unknownCount = MapPointTypeCounts.StandardRandomUnknownCount(mapRng) - 1;
        return new MapPointTypeCounts(unknownCount, restCount);
    }
}