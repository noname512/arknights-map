using ArknightsMap.Scripts.Cards;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Powers;

[RegisterPower]
public class HiddenPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override PowerAssetProfile AssetProfile =>
        new(IconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png", BigIconPath: $"res://ArknightsMap/images/powers/{GetType().Name}.png");

    private bool shouldTrigger => !CreaturePositions.IsBlock(Owner, Target);
    private static readonly List<CardModel> chosenCards = [ModelDb.Card<ComeClose>(), ModelDb.Card<RunAway>()];
    public override bool ShouldAllowTargeting(Creature target)
    {
        if ((target != Owner) || !IsVisible)
        {
            return true;
        }
        return !shouldTrigger;
    }
    
    public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner)
        {
            return amount;
        }
        if (dealer != Target)
        {
            return amount;
        }

        if (!shouldTrigger)
        {
            return amount;
        }
        return 0;
    }
    
    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? _, out decimal modifiedAmount)
    {
        if (target != base.Owner)
        {
            modifiedAmount = amount;
            return false;
        }

        if ((canonicalPower.Applier != Target) || (!shouldTrigger))
        {
            modifiedAmount = amount;
            return false;
        }
        if (canonicalPower.GetTypeForAmount(amount) != PowerType.Debuff)
        {
            modifiedAmount = amount;
            return false;
        }
        if (!canonicalPower.IsVisible)
        {
            modifiedAmount = amount;
            return false;
        }
        modifiedAmount = default(decimal);
        return true;
    }


    private async Task ChooseBlockOrNot()
    {
        if (Target.IsDead)
        {
            return;
        }

        List<CardModel> cards = [];
        foreach (CardModel card in chosenCards)
        {
            CardModel card2 = CombatState.CreateCard(card, Target.Player);
            cards.Add(card2);
        }
        CardModel cardModel = await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), cards, Target.Player);
        if (cardModel != null)
        {
            await ((KnowledgeDemon.IChoosable)cardModel).OnChosen();
        }
    }
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            await ChooseBlockOrNot();
        }
    }
}
