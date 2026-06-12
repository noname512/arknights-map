
using Godot;
using ArknightsMap.Scripts.Ancients;
using ArknightsMap.Scripts.Encounters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Random;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ArknightsMap.Scripts.Acts;

[RegisterAct]
public sealed class SnowyMountain : ModActTemplate
{
    public override string[] MusicBankPaths => ["res://ArknightsMap/audio/ArknightsMap.bank", "res://ArknightsMap/audio/ArknightsMap.bank"];
    public override string[] BgMusicOptions => ["event:/ArknightsMap/music/wilds_bg_1", "event:/ArknightsMap/music/wilds_bg_2"];

    public override Color MapTraveledColor => new Color("27221C");

    public override Color MapUntraveledColor => new Color("6E7750");

    public override Color MapBgColor => new Color("9B9562");

    public override string ChestSpineResourcePath => "res://animations/backgrounds/treasure_room/chest_room_act_3_skel_data.tres";
    public override string ChestSpineSkinNameNormal => "act3";
    public override string ChestSpineSkinNameStroke => "act3_stroke";
    public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act3";
    public override string AmbientSfx => "event:/sfx/ambience/act3_ambience";
    protected override int BaseNumberOfRooms => 14;

    public override string? CustomBackgroundScenePath => "res://ArknightsMap/scenes/acts/SnowyMountain/snowy_mountain.tscn";
    public override string? CustomBackgroundLayersDirectoryPath => "res://ArknightsMap/scenes/acts/SnowyMountain/layers";
    public override string? CustomMapTopBgPath => "res://images/packed/map/map_bgs/hive/map_top_hive.png";
    public override string? CustomMapMidBgPath => "res://images/packed/map/map_bgs/hive/map_middle_hive.png";
    public override string? CustomMapBotBgPath => "res://images/packed/map/map_bgs/hive/map_bottom_hive.png";
    public override string? CustomRestSiteBackgroundPath => "res://scenes/rest_site/hive_rest_site.tscn";

    public override IEnumerable<EventModel> AllEvents => new EventModel[]
    {
        // ModelDb.Event<TheWake>(),
    };

    public override IEnumerable<AncientEventModel> AllAncients => new AncientEventModel[]
    {
        ModelDb.AncientEvent<Pramanix>(),
    };

    public override IEnumerable<EncounterModel> BossDiscoveryOrder => new EncounterModel[]
    {
        ModelDb.Encounter<MandragoraBoss>(),
    };

    public override IEnumerable<EncounterModel> GenerateAllEncounters() => new EncounterModel[]
    {
        ModelDb.Encounter<BurnTheHaystacks>(),
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