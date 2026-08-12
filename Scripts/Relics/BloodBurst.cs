using ArknightsMap.Scripts.Enchantments;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class BloodBurst : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (CombatManager.Instance.IsInProgress && target == Owner.Creature && dealer != null && dealer.Monster != null && result.UnblockedDamage > 0)
        {
            Flash();
            for (int i = 0; i < DynamicVars.Cards.BaseValue; i++)
            {
                await CardPileCmd.Draw(choiceContext, Owner);
            }

            await AutoPlayAttackCards(choiceContext, dealer);
        }
    }

    public async Task AutoPlayAttackCards(PlayerChoiceContext choiceContext, Creature target)
    {
        if (CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }
        Log.Info("Hello!");
        List<CardModel> cards = new List<CardModel>();
        foreach (CardModel card in PileType.Hand.GetPile(Owner).Cards)
        {
            if (card.Type == CardType.Attack)
            {
                Log.Info("Try Add " + card.Title);
                cards.Add(card);
                Log.Info("Added " + card.Title);
            }
        }
        foreach (CardModel card in cards)
        {
            await CardPileCmd.Add(card, PileType.Play);
        }
        Log.Info("CardPileCmd.Add OK");
        foreach (CardModel card in cards)
        {
            if (!card.Owner.Creature.IsDead)
            {
                if ((card.TargetType == TargetType.AnyEnemy) && (!target.IsDead))
                {
                    await CardCmd.AutoPlay(choiceContext, card, target);
                }
                else
                {
                    await CardCmd.AutoPlay(choiceContext, card, null);
                }
                Log.Info("Play " + card.Title);
                continue;
            }
            break;
        }
    }
}
