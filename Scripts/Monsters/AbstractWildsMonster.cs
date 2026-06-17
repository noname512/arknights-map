using STS2RitsuLib.Scaffolding.Content;

public abstract class AbstractWildsMonster : ModMonsterTemplate
{
    public virtual async Task OnReedBedStatusChange(bool burning) { }
}
