using ArknightsMap.Scripts.Powers;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class Proliferate : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromKeyword(ReedBedKeyword.Keyword), HoverTipFactory.FromPower<FlamingDamagePower>(), HoverTipFactory.FromPower<VulnerablePower>()];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );
}
