using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public sealed class PreciseMachine : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2), new CardsVar(2)];

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

    public int SuccessCount = 0;

    public override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner)
        {
            if (SuccessCount == 0 && cardPlay.Card.Type == CardType.Attack)
            {
                SuccessCount++;
            }
            else if (SuccessCount == 1 && cardPlay.Card.Type == CardType.Skill)
            {
                SuccessCount++;
            }
            else if (SuccessCount == 2 && cardPlay.Card.Type == CardType.Power)
            {
                SuccessCount++;
                await PlayerCmd.GainEnergy(2, Owner);
                await CardPileCmd.Draw(choiceContext, 2, Owner);
                SuccessCount = 0;
            }
            else
            {
                // 只有不符合任何顺序要求时才重置（例如该出Attack时出了Skill，或该出Skill时出了Attack）
                SuccessCount = 0;
            }
        }

        InvokeDisplayAmountChanged();
    }

    public override async Task BeforeCombatStart()
    {
        SuccessCount = 0;
    }

    public override int DisplayAmount
    {
        get => SuccessCount;
    }
}
