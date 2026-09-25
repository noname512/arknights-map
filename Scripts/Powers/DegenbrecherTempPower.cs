using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class DegenbrecherTempPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Power<LowStrengthPower>();
    public override LocString Title => ((PowerModel)OriginModel).Title;
    protected override bool IsPositive => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<StrengthPower>();
}
