using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class ShieldPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Time", 3)];

    public override int DisplayAmount => (int)DynamicVars["Time"].BaseValue;

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target?.Side != CombatSide.Player)
        {
            return 1m;
        }

        if (!props.IsPoweredAttack())
        {
            return 1m;
        }
        if (dealer != Owner)
        {
            return 1m;
        }
        if ((int)DynamicVars["Time"].BaseValue == 0)
        {
            return 1m;
        }
        return 0.2m;
    }

    public override decimal ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
    {
        if (power.Type == PowerType.Debuff && (int)DynamicVars["Time"].BaseValue > 0)
        {
            DynamicVars["Time"].UpgradeValueBy(-1);
            return 0m;
        }
        return base.ModifyPowerAmountGivenAdditive(power, giver, amount, target, cardSource);
    }
}