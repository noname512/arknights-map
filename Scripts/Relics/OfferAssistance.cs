using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class OfferAssistance : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public override bool IsAllowed(IRunState runState)
    {
        return runState.Players.Count > 1;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
            return;
        IEnumerable<CardModel> customCardPool = from c in ModelDb.AllCards where c.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly select c;
        List<CardModel> options = CardFactory
            .GetDistinctForCombat(Owner, customCardPool, (int)DynamicVars.Cards.BaseValue, Owner.RunState.Rng.CombatCardGeneration)
            .ToList();
        CardModel? chosenCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner, canSkip: true);
        if (chosenCard != null)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(chosenCard, PileType.Hand));
        }
    }
}
