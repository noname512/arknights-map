using ArknightsMap.Scripts.Ancients;
using ArknightsMap.Scripts.Cards;
using ArknightsMap.Scripts.Encounters;
using ArknightsMap.Scripts.Events;
using ArknightsMap.Scripts.Monsters;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Test.Scripts;

namespace ArknightsMap.Scripts.Acts;

[RegisterAct]
public sealed class Laterano : ModActTemplate
{
    public override string[] MusicBankPaths => ["res://ArknightsMap/audio/ArknightsMap.bank", "res://ArknightsMap/audio/ArknightsMap.bank"];
    public override string[] BgMusicOptions => ["event:/ArknightsMap/music/laterano_bg"];

    public override Color MapTraveledColor => new Color("27221C");

    public override Color MapUntraveledColor => new Color("6E7750");

    public override Color MapBgColor => new Color("9B9562");
    protected override int NumberOfWeakEncounters => 2;
    public override int Index => 3;
    public override bool IsDefault => false;

    public override bool IsUnlocked(UnlockState unlockState) => false;

    public override string ChestSpineResourcePath => "res://animations/backgrounds/treasure_room/chest_room_act_3_skel_data.tres";
    public override string ChestSpineSkinNameNormal => "act3";
    public override string ChestSpineSkinNameStroke => "act3_stroke";
    public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act3";
    public override string AmbientSfx => "event:/sfx/ambience/act3_ambience";
    protected override int BaseNumberOfRooms => 14;

    public override string? CustomBackgroundScenePath => "res://ArknightsMap/scenes/acts/Laterano/laterano_background.tscn";
    public override string? CustomBackgroundLayersDirectoryPath => "res://ArknightsMap/scenes/acts/Laterano/layers";
    public override string? CustomMapTopBgPath => "res://images/packed/map/map_bgs/glory/map_top_glory.png";
    public override string? CustomMapMidBgPath => "res://images/packed/map/map_bgs/glory/map_middle_glory.png";
    public override string? CustomMapBotBgPath => "res://images/packed/map/map_bgs/glory/map_bottom_glory.png";
    public override string? CustomRestSiteBackgroundPath => "res://scenes/rest_site/glory_rest_site.tscn";

    public override IEnumerable<EventModel> AllEvents =>
        new EventModel[]
        {
            ModelDb.Event<StephenZone>(),
            ModelDb.Event<FirstClass>(),
        };

    public override IEnumerable<AncientEventModel> AllAncients =>
        new AncientEventModel[]
        {
            ModelDb.AncientEvent<Paganini>(), // 潘格尼尼
            ModelDb.AncientEvent<Lemuen>(), // 蕾缪安
            ModelDb.AncientEvent<Gracebearer>(), //聆音
            //ModelDb.AncientEvent<Executor>(), // 送葬人
        };

    public override IEnumerable<EncounterModel> BossDiscoveryOrder =>
        new EncounterModel[]
        {
            ModelDb.Encounter<SSSBoss>(),
            ModelDb.Encounter<TheSaintBoss>(),
            ModelDb.Encounter<OpForGunBoss>(),
        };

    public override IEnumerable<EncounterModel> GenerateAllEncounters() =>
        new EncounterModel[]
        {
            ModelDb.Encounter<WasteLanders>(),
            ModelDb.Encounter<SingleSanktaBlade>(),
            ModelDb.Encounter<SinglePathfinderCannon>(),
            ModelDb.Encounter<ThreeWarriors>(),
            ModelDb.Encounter<SingleSanktaClaw>(), 
            ModelDb.Encounter<ThreeEyes>(),


            ModelDb.Encounter<SinglePathfinderCar>(),
            ModelDb.Encounter<SingleWastelandPlunder>(),
            ModelDb.Encounter<TwoBHCrossbow>(),
            ModelDb.Encounter<TwoMage>(),
            ModelDb.Encounter<TwoWarriors>(),
            ModelDb.Encounter<ThreeEyesWithPride>(),
            ModelDb.Encounter<ClawAndStatue>(),
            ModelDb.Encounter<BladeAndStatue>(),


            ModelDb.Encounter<FortunaElite>(),
            ModelDb.Encounter<OrenElite>(),
            ModelDb.Encounter<ThreeSanktas>(),            
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

    public override async Task<Task> AfterActEntered()
    {
        var RunState = RunManager.Instance.DebugOnlyGetState();
        if (RunState == null)
        {
            return base.AfterActEntered();
        }
        foreach (Player p in RunState.Players)
        {
            await CardPileCmd.AddCurseToDeck<Confused>(p);
        }
        return base.AfterActEntered();
    }


    

}
