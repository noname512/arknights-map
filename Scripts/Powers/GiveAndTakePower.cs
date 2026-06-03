using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class GiveAndTakePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Exceed", 0)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<FlamingDamagePower>();
    public override int DisplayAmount => DynamicVars["Exceed"].IntValue;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png"
    );

    public void Exceed(decimal amount)
    {
        DynamicVars["Exceed"].BaseValue += amount;
        InvokeDisplayAmountChanged();
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (Owner != dealer)
        {
            return 0;
        }

        if (target != Target)
        {
            return 0;
        }
        
        if (!props.IsPoweredAttack())
        {
            return 0;
        }
        if (Owner.Monster.NextMove.StateId == "RETURN_FIRE1")
        {
            return DynamicVars["Exceed"].BaseValue / 2;
        }
        if (Owner.Monster.NextMove.StateId == "RETURN_FIRE2")
        {
            return DynamicVars["Exceed"].BaseValue;
        }
        return 0;
    }

    public void Return(decimal amount)
    {
        DynamicVars["Exceed"].BaseValue -= amount;
        InvokeDisplayAmountChanged();
    }
}