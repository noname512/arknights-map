using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Cards;

[RegisterCard(typeof(TokenCardPool))]
public class EstinguishedEmergencyHeater : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Status;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    public override int MaxUpgradeLevel => 0;
    private bool shouldTransform = false;

    public EstinguishedEmergencyHeater()
        : base(energyCost, type, rarity, targetType) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromCard<EmergencyHeater>()];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        shouldTransform = true;
        return Task.CompletedTask;
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if ((card == this) && (shouldTransform))
        {
            await CardCmd.TransformTo<EmergencyHeater>(this);
        }
    }

    protected override CardLocation GetResultLocationForCardPlay()
    {
        CardLocation resultLocationForCardPlay = base.GetResultLocationForCardPlay();
        if (resultLocationForCardPlay.pileType == PileType.Discard)
        {
            resultLocationForCardPlay.pileType = PileType.Hand;
        }
        return resultLocationForCardPlay;
    }
    
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
        {
            EnergyCost.AddThisCombat(-1);
        }
    }
}
