using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Events;

[RegisterActEvent(typeof(Laterano))] 

public sealed class StephenZone : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(InitialPortraitPath: $"res://ArknightsMap/images/events/{GetType().Name}.png");
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        
    ];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, IceCream, InitialOptionKey("ICE_CREAM"), [.. HoverTipFactory.FromRelic<IcecreamMachine>()]),
        new EventOption(this, GainRelic, InitialOptionKey("GAIN_RELIC")),
    ];

    // 失去生命
    private async Task IceCream()
    {
        await RelicCmd.Obtain<IcecreamMachine>(Owner!);
        await GainTwoRelic();
    }

    

    // 获得生命
    private async Task GainRelic()
    {
        await RewardsCmd.OfferCustom(Owner!, [new RelicReward(Owner!)]);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.GAIN_RELIC.description"));
    }

    // 进入事件第二阶段，两个选项：选择药水或者选择卡牌

    private async Task GainTwoRelic()
    {
        await RewardsCmd.OfferCustom(Owner!, [new RelicReward(Owner!),new RelicReward(Owner!)]);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.GAIN_TWO_RELIC.description"));

    }


    public override bool IsAllowed(IRunState runState)
    {
        return runState.Act is Laterano && runState.ActFloor < 8;
    }
    



    
}