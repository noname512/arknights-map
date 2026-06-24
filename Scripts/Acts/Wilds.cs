using ArknightsMap.Scripts.Ancients;
using ArknightsMap.Scripts.Encounters;
using ArknightsMap.Scripts.Events;
using Godot;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Unlocks;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Acts;

[RegisterAct]
public sealed class Wilds : ModActTemplate
{
    public override string[] MusicBankPaths => ["res://ArknightsMap/audio/ArknightsMap.bank", "res://ArknightsMap/audio/ArknightsMap.bank"];
    public override string[] BgMusicOptions => ["event:/ArknightsMap/music/wilds_bg_1", "event:/ArknightsMap/music/wilds_bg_2"];

    public override Color MapTraveledColor => new Color("27221C");

    public override Color MapUntraveledColor => new Color("6E7750");

    public override Color MapBgColor => new Color("9B9562");
    protected override int NumberOfWeakEncounters => 2;
    public override int Index => 1;
    public override bool IsDefault => false;

    public override bool IsUnlocked(UnlockState unlockState) => false;

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

    public override IEnumerable<EventModel> AllEvents =>
        new EventModel[]
        {
            ModelDb.Event<TheWake>(), // 守灵仪式
            ModelDb.Event<Campfire>(), // 篝火？
            ModelDb.Event<TheLeaderOfDublinn>(), // 深池的“领袖”
            ModelDb.Event<OverlookingNasaoirsi>(), // 俯瞰纳斯尔纱
            ModelDb.Event<HaystackMidnightTalks>(), // 草垛夜话
            ModelDb.Event<IntelligenceBroker>(), // 情报贩卖商
            ModelDb.Event<ChanceEncounterWithCannot>(), // 偶遇坎诺特
        };

    public override IEnumerable<AncientEventModel> AllAncients =>
        new AncientEventModel[]
        {
            ModelDb.AncientEvent<Reed>(), // 苇草
            ModelDb.AncientEvent<Bagpipe>(), // 风笛
            ModelDb.AncientEvent<Tezcatara>(), // 稻草人
        };

    public override IEnumerable<EncounterModel> BossDiscoveryOrder =>
        new EncounterModel[]
        {
            ModelDb.Encounter<MandragoraBoss>(), // 蔓德拉
            ModelDb.Encounter<AFRBoss>(), // “万火归一”
            ModelDb.Encounter<HerFlame>(), // “领袖”
        };

    public override IEnumerable<EncounterModel> GenerateAllEncounters() =>
        new EncounterModel[]
        {
            ModelDb.Encounter<BurnTheHaystacks>(),
            ModelDb.Encounter<ScorchingDream>(),
            ModelDb.Encounter<DublinnPhalanx>(),
            ModelDb.Encounter<DublinnPhalanxWeak>(),
            ModelDb.Encounter<PatrollingFoliageNormal>(),
            ModelDb.Encounter<DublinnCompanion>(),
            ModelDb.Encounter<MandragoraBoss>(),
            ModelDb.Encounter<ScaldingEarth>(),
            ModelDb.Encounter<ApparitionalWaves>(),
            ModelDb.Encounter<ComingFire>(),
            ModelDb.Encounter<EndOfTheNight>(),
            ModelDb.Encounter<AFRBoss>(),
            ModelDb.Encounter<FloralGarland>(),
            ModelDb.Encounter<LampBurner>(),
            ModelDb.Encounter<HerFlame>(),
            ModelDb.Encounter<MarchOfTheDead>(),
            ModelDb.Encounter<BurningWeeds>(),
            ModelDb.Encounter<DublinnFlamerazerNormal>(),
            ModelDb.Encounter<AshyMarsh>(),
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
