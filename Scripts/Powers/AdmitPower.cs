using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class AdmitPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool ShouldScaleInMultiplayer => true;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Hit",1),
        new IntVar("Current",0),
    ];

    public int Current
    {
        set
        {
            DynamicVars["Current"].BaseValue = value;
            InvokeDisplayAmountChanged();
        }
        get
        {
            return DynamicVars["Current"].IntValue;
        }
    }

    public override int DisplayAmount => Current;

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if ((dealer == Owner) && (target.IsPlayer) && (props.IsPoweredAttack()) && result.UnblockedDamage == 0)
        {
            Flash();
            await AddAdmit(DynamicVars["Hit"].IntValue);
        }
    }

    public async Task AddAdmit(int num)
    {
        Current += num;
        if (Current >= Amount)
        {
            await CreatureCmd.Kill(Owner);
        }
    }
}
