using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class MomentumMurder : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("TurnLeft", 0),
        new IntVar("CannotAttack", 2),
        new IntVar("Admit", 6)
    ];

    public int TurnLeft
    {
        get
        {
            return DynamicVars["TurnLeft"].IntValue;
        }
        set
        {
            DynamicVars["TurnLeft"].BaseValue = value;
        }
    }
    
    public int Admit
    {
        get
        {
            return DynamicVars["Admit"].IntValue;
        }
        set
        {
            DynamicVars["Admit"].BaseValue = value;
        }
    }
    
    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource
    )
    {
        if (target == Owner)
        {
            for (;Owner.CurrentHp <= Amount;)
            {
                Flash();
                TurnLeft = 2;
                await Owner.GetPower<AdmitPower>().AddAdmit(Admit);
                SetAmount((int)(Amount - Owner.MaxHp * 0.25));
            }
        }
    }

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        if ((TurnLeft > 0) && (card.Type == CardType.Attack))
        {
            return false;
        }
        return base.ShouldPlay(card, autoPlayType);
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if ((side == CombatSide.Enemy) && (TurnLeft > 0))
        {
            TurnLeft--;
        }
        return Task.CompletedTask;
    }
}
