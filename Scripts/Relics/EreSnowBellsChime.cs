using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class EreSnowBellsChime : ModRelicTemplate
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

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
		foreach (CardModel item in await CardSelectCmd.FromDeckGeneric(Owner, new CardSelectorPrefs(L10NLookup("ARKNIGHTS_MAP_RELIC_ERE_SNOW_BELLS_CHIME.choose"), 1)))
		{
			for (int i = 0; i < DynamicVars.Cards.BaseValue; i++)
			{
				CardModel copy = Owner.RunState.CloneCard(item);
				CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(copy, PileType.Deck));
			}
		}
	}
}
