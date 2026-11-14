using ArknightsMap.Scripts.Cards;
using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Dread;
using MegaCrit.Sts2.Core.Rooms;

namespace ArknightsMap.Scripts.Dominions;

public class AllFlamesReturnDominion : DominionModel
{
    public override IEnumerable<IHoverTip> ExtraHoverTips =>
        [.. HoverTipFactory.FromCardWithCardHoverTips<PutOutFire>(), .. HoverTipFactory.FromCardWithCardHoverTips<PurpleFlame>()];

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom combatRoom)
        {
            await PowerCmd.Apply<AllFlamesReturnDominionPower>(
                new ThrowingPlayerChoiceContext(),
                combatRoom.CombatState.Players.Select(p => p.Creature),
                1m,
                null,
                null
            );
        }
    }
}
