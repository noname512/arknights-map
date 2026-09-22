using ArknightsMap.Scripts.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class DegenbrecherTempPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Power<LowStrengthPower>();
    public override LocString Title => ((PowerModel)OriginModel).Title;
    protected override bool IsPositive => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        HoverTipFactory.FromPowerWithPowerHoverTips<StrengthPower>();
}
