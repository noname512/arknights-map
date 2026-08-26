using ArknightsMap.Scripts.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Encounters;

public abstract class AbstractLateranoEncounter : ModEncounterTemplate
{
    public virtual bool isIceCreamCombat => RoomType != MegaCrit.Sts2.Core.Rooms.RoomType.Boss;
    
}
