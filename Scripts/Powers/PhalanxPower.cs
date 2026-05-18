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
public class PhalanxPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png"
    );

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        SetAmount(base.CombatState.Enemies.Count(e => e.IsAlive && e.IsMonster && e.Monster is DublinnPhalanxInfantry) * 2);
        return Task.CompletedTask;
    }

    public override Task AfterCreatureAddedToCombat(Creature creature)
    {
        if (creature.IsMonster && creature.Monster is DublinnPhalanxInfantry)
        {
            SetAmount(Amount + 2);
        }
        return Task.CompletedTask;
    }

    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature.IsMonster && creature.Monster is DublinnPhalanxInfantry)
        {
            SetAmount(Amount - 2);
        }
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (base.Owner != target) return 0;
        return -Amount;
    }
}