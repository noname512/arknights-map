using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Dread;
using MegaCrit.Sts2.Core.Models.Powers;

public class OpForGunDominion : DominionModel
{
    public override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ArtifactPower>(), HoverTipFactory.FromPower<ShieldPower>()];

    protected override async Task AfterEnemiesEnteredCombat(IEnumerable<Creature> creatures)
    {
        await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), creatures, 1m, null, null);
        await PowerCmd.Apply<ShieldPower>(new ThrowingPlayerChoiceContext(), creatures, 1m, null, null);
    }
}