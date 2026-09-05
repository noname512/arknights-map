using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Encounters;

public abstract class AbstractSnowyMountainEncounter : ModEncounterTemplate
{
    public virtual int playerStartPosition => 3;
    public virtual int windBlowTurn => 0;
    public virtual int windBlowDirection => 0;
}
