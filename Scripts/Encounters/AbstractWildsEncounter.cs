using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Encounters;

public abstract class AbstractWildsEncounter : ModEncounterTemplate
{
    public virtual bool isBurningAtStart => false;
}