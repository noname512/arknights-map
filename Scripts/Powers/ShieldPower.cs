using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
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
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Time", 3), new IntVar("Cooldown", 2)];

    public override int DisplayAmount => (int)DynamicVars["Time"].BaseValue;

    

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner)
        {
            return 1m;
        }
        if ((int)DynamicVars["Time"].BaseValue == 0)
        {
            return 1m;
        }
        return 0.5m;
    }

    

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power.Type == PowerType.Debuff && power.Owner == Owner && (int)DynamicVars["Time"].BaseValue > 0 && amount > 0)
        {
            await PowerCmd.Remove(power);
            DynamicVars["Time"].UpgradeValueBy(-1);
            InvokeDisplayAmountChanged();
        }
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Enemy && DynamicVars["Time"].BaseValue == 0)
        {
            DynamicVars["Cooldown"].UpgradeValueBy(-1);
            if ((int)DynamicVars["Cooldown"].BaseValue <= 0)
            {
                DynamicVars["Cooldown"].UpgradeValueBy(2);
                DynamicVars["Time"].UpgradeValueBy(3);
                InvokeDisplayAmountChanged();
            }
        }
        return base.AfterSideTurnStart(side, participants, combatState);
    }
}