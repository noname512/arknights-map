using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class PeaksCladInForest : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    private HashSet<CardType> playedTypes = new HashSet<CardType>();
    private bool triggeredLastTurn = false;
    private CardType type;

    public override Task BeforeCombatStart()
    {
        playedTypes.Clear();
        triggeredLastTurn = false;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (playedTypes.Count == 1)
        {
            triggeredLastTurn = true;
            type = playedTypes.First();
            Status = RelicStatus.Active;
        }
        else
        {
            Status = RelicStatus.Normal;
        }
        playedTypes.Clear();
        return Task.CompletedTask;
    }

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner)
        {
            return count;
        }
        if (!triggeredLastTurn)
        {
            return count;
        }
        return count + DynamicVars.Cards.BaseValue;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!CombatManager.Instance.IsInProgress)
        {
            return Task.CompletedTask;
        }
        if (cardPlay.Card.Owner != Owner)
        {
            return Task.CompletedTask;
        }
        playedTypes.Add(cardPlay.Card.Type);
        if (triggeredLastTurn && cardPlay.Card.Type == type && !cardPlay.IsAutoPlay)
        {
            triggeredLastTurn = false;
        }
        return Task.CompletedTask;
    }

    public override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!ShouldModifyCost(card))
        {
            return false;
        }
        modifiedCost = default(decimal);
        return true;
    }

    public override bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!ShouldModifyCost(card))
        {
            return false;
        }
        modifiedCost = default(decimal);
        return true;
    }

    private bool ShouldModifyCost(CardModel card)
    {
        if (!CombatManager.Instance.IsInProgress)
        {
            return false;
        }
        if (card.Owner.Creature != base.Owner.Creature)
        {
            return false;
        }
        if (!triggeredLastTurn)
        {
            return false;
        }
        if (card.Type != type)
        {
            return false;
        }
        switch (card.Pile?.Type)
        {
            case PileType.Hand:
            case PileType.Play:
                return true;
            default:
                return false;
        }
    }
}
