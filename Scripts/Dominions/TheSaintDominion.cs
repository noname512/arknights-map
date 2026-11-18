using ArknightsMap.Scripts.Cards;
using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Dread;
using MegaCrit.Sts2.Core.Rooms;

namespace ArknightsMap.Scripts.Dominions;

public class TheSaintDominion : DominionModel
{
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ScorchingLightPower>()];

    public override async Task AfterActEntered()
    {
    
        foreach (Player player in base.RunState!.Players)
        {
            await CardPileCmd.AddCurseToDeck<Perplexed>(player);
        }
            
    
    
    }
}
