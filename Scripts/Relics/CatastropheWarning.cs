using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class CatastropheWarning : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Amount", 20)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState
    )
    {
        if (!participants.Contains(Owner.Creature))
        {
            return;
        }
        foreach (Creature c in combatState.Enemies)
        {
            MonsterModel? m = c.Monster;
            if (m != null && m.IntendsToAttack)
            {
                int amount = 0;
                foreach (AbstractIntent intent in m.NextMove.Intents)
                    if (intent is AttackIntent attackIntent)
                        amount += attackIntent.GetTotalDamage([Owner.Creature], c);
                if (amount >= DynamicVars["Amount"].IntValue)
                {
                    Flash();
                    CardModel card = combatState.CreateCard<MegaCrit.Sts2.Core.Models.Cards.Buffer>(Owner);
                    card.AddKeyword(CardKeyword.Ethereal);
                    await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
                    return;
                }
            }
        }
    }
}
