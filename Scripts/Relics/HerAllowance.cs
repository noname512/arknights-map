using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class HerAllowance : ModRelicTemplate
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => [];

	public override RelicAssetProfile AssetProfile => new(
		// 小图标（原版85x85）
		IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
		// 轮廓图标（原版85x85）
		IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
		// 大图标（原版256x256）
		BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
	);

	public override async Task AfterObtained()
	{
		IEnumerable<CardModel> cards = Owner.Character.CardPool.AllCards.Where(c => c.Rarity != CardRarity.Basic && c.Rarity != CardRarity.Event && c.Rarity != CardRarity.Ancient);
		foreach (CardModel item in await CardSelectCmd.FromSimpleGridForRewards(
										prefs: new CardSelectorPrefs(L10NLookup("ARKNIGHTS_MAP_RELIC_HER_ALLOWANCE.choose"), 1),
										context: new BlockingPlayerChoiceContext(),
										cards: cards.Select(c => new CardCreationResult(c)).OrderBy(c => c.Card.Rarity).ToList(),
										player: Owner))
		{
			CardModel newCard = Owner.RunState.CreateCard(item, Owner);
			if (newCard.IsUpgradable) CardCmd.Upgrade(newCard);
			CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(newCard, PileType.Deck));
		}
	}
}
