using ArknightsMap.Scripts.Monsters;
using ArknightsMap.Scripts.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class RebornPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff; 
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png"
    );
    
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
        if (creature != base.Owner)
        {
            return true;
        }
        return !IsReviving;
    }

    public override bool ShouldStopCombatFromEnding()
    {
        return true;
    }

    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
    {
        if (creature != base.Owner)
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