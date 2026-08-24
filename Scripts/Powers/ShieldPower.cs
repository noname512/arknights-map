using ArknightsMap.Scripts.Monsters;
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

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Cooldown", 3)];

    public override int DisplayAmount => Owner.GetPowerAmount<ArtifactPower>();

    

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner)
        {
            return 1m;
        }
        if (Owner.GetPowerAmount<ArtifactPower>() == 0)
        {
            return 1m;
        }
        return 0.5m;
    }

    

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == Owner.GetPower<ArtifactPower>() && power.Owner == Owner && amount < 0)
        {
            InvokeDisplayAmountChanged();
        }
    }

    

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Enemy && Owner.GetPowerAmount<ArtifactPower>() == 0)
        {
            DynamicVars["Cooldown"].UpgradeValueBy(-1);
            if ((int)DynamicVars["Cooldown"].BaseValue <= 0)
            {
                DynamicVars["Cooldown"].UpgradeValueBy(2);
                if (Owner.Monster is OpForGun)
                {
                    await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), Owner, 2, Owner, null);
                }
                else if (Owner.Monster is OpCar)
                {
                    await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), Owner, 1, Owner, null);
                }
                else
                {
                    await PowerCmd.Apply<ArtifactPower>(new ThrowingPlayerChoiceContext(), Owner, 3, Owner, null);
                }
                InvokeDisplayAmountChanged();
            }
        }
    }
}