using ArknightsMap.Scripts.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class ThornsEnhancePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<ThornsPower>()];

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource
    )
    {
        if (target != Owner)
        {
            return;
        }
        if (dealer == null)
        {
            return;
        }
        if (!props.IsPoweredAttack() && cardSource is not Omnislice)
        {
            return;
        }
        if (target.GetPowerAmount<ThornsPower>() <= 0)
        {
            return;
        }
        if (Owner.IsPlayer)
        {
            RelicModel? relic = Owner.Player!.GetRelic<GiantGun>();
            if (relic != null)
                relic.Flash();
        }

        var hittableEnemies = Owner?.CombatState?.HittableEnemies;
        if (Owner == null || hittableEnemies == null)
        {
            return;
        }

        foreach (Creature c in hittableEnemies)
        {
            if (c != dealer)
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    c,
                    Owner.GetPowerAmount<ThornsPower>(),
                    ValueProp.Unpowered | ValueProp.SkipHurtAnim,
                    Owner,
                    null,
                    null
                );
            }
        }
    }
}
