using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class NeverStop : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [];

    public int AttackTime = 0;

    public override int DisplayAmount => AttackTime;

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner && cardPlay.Card.Type == CardType.Attack)
        {
            AttackTime++;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        AttackTime = 0;
    }

    public override async Task AfterSideTurnStartLate(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (AttackTime >= 3)
        {
            await PlayerCmd.GainEnergy(1, Owner);
            List<CardModel> attackCards = new();
            foreach (CardModel c in Owner.PlayerCombatState.DrawPile.Cards)
            {
                if (c.Type == CardType.Attack)
                {
                    attackCards.Add(c);
                }
            }
            if (attackCards.Count > 0)
            {
                CardModel? chosen = attackCards.TakeRandom(1, Owner.RunState.Rng.CombatCardSelection).FirstOrDefault();
                if (chosen != null)
                {
                    await CardPileCmd.Add(chosen, PileType.Draw, CardPilePosition.Top);
                    await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), Owner);
                }
            }
        }
    }

    
}