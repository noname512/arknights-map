using ArknightsMap.Scripts.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class WitherPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Relic<MarkOfWither>();

    protected override bool IsPositive => false;
    private bool _isFirstTurn = true;
    
    public override async Task BeforeApplied(
        Creature target,
        Decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        _isFirstTurn = true;
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), target, -amount, applier, cardSource, true);
    }
    
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        TemporaryStrengthPower power = this;
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