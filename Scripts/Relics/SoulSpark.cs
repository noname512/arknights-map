using Godot;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class SoulSpark : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override RelicAssetProfile AssetProfile => new(
        // 小图标（原版85x85）
        IconPath: $"res://Test/images/relics/{GetType().Name}.png",
        // 轮廓图标（原版85x85）
        IconOutlinePath: $"res://Test/images/relics/{GetType().Name}.png",
        // 大图标（原版256x256）
        BigIconPath: $"res://Test/images/relics/{GetType().Name}.png"
    );

    public override async Task BeforeCombatStart()
    {
        
    }
}
