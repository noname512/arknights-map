using ArknightsMap.Scripts.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class WitherPower : ModPowerTemplate, ITemporaryPower
{
    public AbstractModel OriginModel => ModelDb.Relic<MarkOfWither>();
    public PowerModel InternallyAppliedPower => ModelDb.Power<StrengthPower>();

    public void IgnoreNextInstance() { }

    /* 上面的是给ITemporary看的，避免被复活之类的清除用的 */

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    public override int DisplayAmount => TurnLeft;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromPowerWithPowerHoverTips<StrengthPower>();
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("TurnLeft", 2)];

    public int TurnLeft
    {
        set
        {
            DynamicVars["TurnLeft"].BaseValue = value;
            InvokeDisplayAmountChanged();
        }
        get { return DynamicVars["TurnLeft"].IntValue; }
    }

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    public override async Task BeforeApplied(Creature target, Decimal amount, Creature? applier, CardModel? cardSource)
    {
        TurnLeft = 2;
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), target, -amount, applier, cardSource, true);
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        WitherPower power = this;
        if (side != power.Owner.Side)
            return;
        if ((Owner.Monster != null) && (!Owner.Monster.IntendsToAttack))
        {
            return;
        }

        TurnLeft--;
        Flash();
        if (TurnLeft == 0)
        {
            await PowerCmd.Remove(power);
            await PowerCmd.Apply<StrengthPower>(choiceContext, power.Owner, power.Amount, power.Owner, null);
        }
    }
}
