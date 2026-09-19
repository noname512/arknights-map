using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class AvalanchePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Threshold", 30), new HpLossVar(400), new IntVar("Current", 0)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [];
    public override int DisplayAmount => DynamicVars["Current"].IntValue;

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    public override async Task AfterDamageReceivedLate(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource
    )
    {
        if (target == Owner && dealer != Owner)
        {
            DynamicVars["Current"].BaseValue += result.UnblockedDamage;
            if (DynamicVars["Current"].IntValue >= DynamicVars["Threshold"].IntValue)
            {
                int currentPos = CreaturePositions.PositionOfCreature(Owner);
                if (currentPos == 9)
                {
                    await CreatureCmd.Damage(choiceContext, Owner, DynamicVars.HpLoss.IntValue, ValueProp.Unblockable | ValueProp.Unpowered, Owner);
                }
                else
                {
                    await CreaturePositions.MoveTo(Owner, currentPos + 1);
                }
                DynamicVars["Current"].BaseValue = 0;
            }
            InvokeDisplayAmountChanged();
        }
    }
}
