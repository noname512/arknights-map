using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Relics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Events;

//[RegisterActEvent(typeof(Laterano))] 

public sealed class WorkSpace : ModEventTemplate
{
    public override EventAssetProfile AssetProfile => new(InitialPortraitPath: $"res://ArknightsMap/images/events/{GetType().Name}.png");
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        
        new IntVar("Gold_Upgrade", 100),
        new IntVar("Gold_Remove", 200),
        
        
    ];

    

    
    

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        Owner!.Gold >= 100
            ? new EventOption(this, Upgrade, InitialOptionKey("UPGRADE"))
            : new EventOption(this, null, InitialOptionKey("UPGRADE_LOCKED")),
        Owner!.Gold >= 200
            ? new EventOption(this, Remove, InitialOptionKey("REMOVE"))
            : new EventOption(this, null, InitialOptionKey("REMOVE_LOCKED")),
        new EventOption(this, Ice, InitialOptionKey("ICE")),
    ];

    
    private async Task Upgrade()
    {
        await PlayerCmd.LoseGold(100, Owner!);
        foreach (
            CardModel item in await CardSelectCmd.FromDeckForUpgrade(
                prefs: new CardSelectorPrefs(),
                player: Owner!
                
            )
        )
        {
            CardCmd.Upgrade(item);
        }
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.UPGRADE.description"));
    }

    private async Task Remove()
    {
        await PlayerCmd.LoseGold(200, Owner!);
        foreach (
            CardModel item in await CardSelectCmd.FromDeckForRemoval(
                prefs: new CardSelectorPrefs(CardSelectorPrefs.RemoveSelectionPrompt, DynamicVars.Cards.IntValue),
                player: Owner!
            )
        )
        {
            await CardPileCmd.RemoveFromDeck(item);
            
        }
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.REMOVE.description"));
    }



    

    
    

    private async Task Ice()
    {
        await CreatureCmd.Heal(Owner!.Creature, 5);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.ICE.description"));
    }


    // 进入事件第二阶段，两个选项：选择药水或者选择卡牌

    


    public override bool IsAllowed(IRunState runState)
    {
        return runState.Act is Laterano ;
    }
    


}