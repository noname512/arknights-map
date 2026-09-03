using ArknightsMap.Scripts.Enchantments;
using ArknightsMap.Scripts.Powers;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class BreadWithSugar : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1), new EnergyVar("Energy2", 1)];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    private int _remainTimes;
    public override bool ShowCounter => true;

    [SavedProperty]
    public int RemainTimes
    {
        get { return _remainTimes; }
        private set
        {
            AssertMutable();
            _remainTimes = value;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (RemainTimes > 0)
        {
            Status = RelicStatus.Normal;
        }
        else
        {
            Status = RelicStatus.Disabled;
        }
        InvokeDisplayAmountChanged();
    }

    public override int DisplayAmount => RemainTimes;

    public override Task AfterObtained()
    {
        RemainTimes = 3;
        return Task.CompletedTask;
    }

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if ((Owner.Creature.CurrentHp <= Owner.Creature.MaxHp * 0.5F) && (RemainTimes > 0))
        {
            Flash();
            await CreatureCmd.Heal(Owner.Creature, Owner.Creature.MaxHp);
            RemainTimes--;
        }
    }
}
