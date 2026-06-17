using ArknightsMap.Scripts.Monsters;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class RebornPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    private class Data
    {
        public bool isReviving;
    }

    private bool IsReviving => GetInternalData<Data>().isReviving;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public void DoRevive()
    {
        GetInternalData<Data>().isReviving = false;
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (!wasRemovalPrevented && creature == Owner)
        {
            GetInternalData<Data>().isReviving = true;
            if (creature.Monster is TombkeeperGrotesque tombkeeperGrotesque)
            {
                tombkeeperGrotesque.TriggerDeadState();
            }
            if (creature.Monster is AllFlamesReturned allFlamesReturned)
            {
                await allFlamesReturned.TriggerDeadState();
            }
        }
    }

    public override bool ShouldAllowHitting(Creature creature)
    {
        if (creature != Owner)
        {
            return true;
        }
        return !IsReviving;
    }

    public override bool ShouldAllowTargeting(Creature target)
    {
        if (target != Owner)
        {
            return true;
        }
        return !IsReviving;
    }

    public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner)
        {
            return amount;
        }
        if (!IsReviving)
        {
            return amount;
        }

        return 0;
    }

    public override bool ShouldStopCombatFromEnding()
    {
        return true;
    }

    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
    {
        if (creature != Owner)
        {
            return true;
        }
        return false;
    }

    public override bool ShouldPowerBeRemovedAfterOwnerDeath()
    {
        return false;
    }

    public override bool ShouldPowerBeRemovedOnDeath(PowerModel power)
    {
        if (power.Type == PowerType.Debuff)
        {
            return true;
        }
        return false;
    }
}
