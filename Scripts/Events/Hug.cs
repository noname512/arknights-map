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

public sealed class Hug : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(InitialPortraitPath: $"res://ArknightsMap/images/events/{GetType().Name}.png");
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(13m, ValueProp.Unblockable | ValueProp.Unpowered), 
        new IntVar("Health", 13),
        new IntVar("Gold", 66),
        
    ];

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, Flower, InitialOptionKey("FLOWER")),
        new EventOption(this, Kick, InitialOptionKey("KICK"), [.. HoverTipFactory.FromRelic<TheSounds>()]),
    ];

    // 失去生命
    private async Task Flower()
    {
        await PlayerCmd.LoseGold(66, Owner!);
        await CreatureCmd.Heal(Owner!.Creature, 13);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.FLOWER.description"));
    }

    

    // 获得生命
    private async Task Kick()
    {
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature, DynamicVars.Damage, null, null);
        await RelicCmd.Obtain<TheSounds>(Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.KICK.description"));
    }

    // 进入事件第二阶段，两个选项：选择药水或者选择卡牌

    


    public override bool IsAllowed(IRunState runState)
    {
        return runState.Act is Laterano ;
    }
    


}