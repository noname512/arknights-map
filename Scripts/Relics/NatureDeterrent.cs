using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class NatureDeterrent : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VulnerablePower>(1)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    private HashSet<Creature> attackTargets = new HashSet<Creature>();
    private bool isFirstAttack;
    private CardModel? firstAttackCard;

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState
    )
    {
        if (participants.Contains(base.Owner.Creature))
        {
            isFirstAttack = true;
            firstAttackCard = null;
        }
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (!CombatManager.Instance.IsInProgress)
        {
            return Task.CompletedTask;
        }
        if (cardPlay.Card.Owner != Owner)
        {
            return Task.CompletedTask;
        }
        if (cardPlay.Card.Type == CardType.Attack && isFirstAttack)
        {
            attackTargets.Clear();
            firstAttackCard = cardPlay.Card;
        }
        return Task.CompletedTask;
    }

    public override Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource
    )
    {
        if (firstAttackCard != null && cardSource == firstAttackCard && target.IsMonster)
        {
            attackTargets.Add(target);
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!CombatManager.Instance.IsInProgress)
        {
            return;
        }
        if (cardPlay.Card.Owner != Owner)
        {
            return;
        }
        if (cardPlay.Card.Type == CardType.Attack && isFirstAttack)
        {
            Flash();
            await PowerCmd.Apply<VulnerablePower>(choiceContext, attackTargets.ToList(), 1, Owner.Creature, null);
            isFirstAttack = false;
        }
    }
}
