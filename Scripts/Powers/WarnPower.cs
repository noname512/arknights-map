using ArknightsMap.Scripts.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Test.Scripts.Powers;

// 注册power并设置Inherit = true，使得继承这个类的power自动被注册
[RegisterPower(Inherit = true)]
public class WarnPower : ModTemporaryAppliedPowerTemplate<Warn, StrengthPower>
{
    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    
    
}