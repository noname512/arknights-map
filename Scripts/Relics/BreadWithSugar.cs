using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
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
        if (creature != Owner.Creature)
        {
            return;
        }
        if (creature.CurrentHp <= creature.MaxHp * 0.5f && RemainTimes > 0)
        {
            Flash();
            await CreatureCmd.Heal(creature, creature.MaxHp);
            RemainTimes--;
        }
    }
}
