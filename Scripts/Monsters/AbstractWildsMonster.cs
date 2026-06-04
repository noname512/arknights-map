using STS2RitsuLib.Scaffolding.Content;

public abstract class AbstractWildsMonster : ModMonsterTemplate
{
    public async virtual Task OnReedBedStatusChange(bool burning) { }
}