using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public class Vengeance : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("DamageIncrease", 40)];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer != Owner.Creature)
        {
            return 1m;
        }
        if (target == null || target.Monster == null)
        {
            return 1m;
        }
        if (!target.Monster.IntendsToAttack)
        {
            return 1m;
        }
        if (!props.IsPoweredAttack())
        {
            return 1m;
        }
        return 1 + DynamicVars["DamageIncrease"].IntValue * 0.01m;
    }
}
