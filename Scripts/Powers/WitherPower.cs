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
public class WitherPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff; 
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<StrengthPower>();

    private bool _isFirstTurn = true;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png"
    );
    
    public override async Task BeforeApplied(
        Creature target,
        Decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        _isFirstTurn = true;
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), target, -amount, applier, cardSource, true);
    }
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        WitherPower power = this;
        if (side != power.Owner.Side)
            return;
        if (_isFirstTurn)
        {
            _isFirstTurn = false;
            return;
        }
        Flash();
        await PowerCmd.Remove(power);
        await PowerCmd.Apply<StrengthPower>(choiceContext, power.Owner, power.Amount, power.Owner, null);
    }
    
}