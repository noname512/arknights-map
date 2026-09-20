using STS2RitsuLib.Scaffolding.Content;

public abstract class AbstractSnowyMountainMonster : ModMonsterTemplate
{
    public virtual Task OnWindBlow()
    {
        return Task.CompletedTask;
    }
}
