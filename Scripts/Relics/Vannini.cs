using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class Vannini : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public override bool ShowCounter => true;

    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (player != Owner)
        {
            return amount;
        }
        return amount + DynamicVars.Energy.IntValue;
    }

    public int SameTypeCount = 0;

    public CardType cardType = CardType.None;

    public override Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner)
        {
            if (SameTypeCount == 0)
            {
                SameTypeCount++;
                cardType = cardPlay.Card.Type;
            }
            else if (cardPlay.Card.Type == cardType)
            {
                SameTypeCount++;
            }
            else
            {
                SameTypeCount = 1;
                cardType = cardPlay.Card.Type;
            }
        }
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public override Task AfterCombatVictory(CombatRoom room)
    {
        SameTypeCount = 0;
        cardType = CardType.None;
        return Task.CompletedTask;
    }

    public override int DisplayAmount
    {
        get => SameTypeCount;
    }

    private bool ShouldPreventCardPlay(CardModel card)
    {
        return SameTypeCount == 3 && card.Type == cardType;
    }

    public override bool ShouldPlay(CardModel card, AutoPlayType _)
    {
        if (card.Owner != Owner)
        {
            return true;
        }
        return !ShouldPreventCardPlay(card);
    }
}
