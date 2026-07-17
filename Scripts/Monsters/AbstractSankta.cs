using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Scaffolding.Content;

public abstract class AbstractSankta : ModMonsterTemplate
{
    public virtual int Bullet => 0;

    public virtual int BulletMax => 0;

    public override async Task AfterAddedToRoom()
    {
        await PowerCmd.Apply<BulletPower>(new ThrowingPlayerChoiceContext(), Creature, Bullet, Creature, null);
    }
}
