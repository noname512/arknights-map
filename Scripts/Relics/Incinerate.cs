using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(EventRelicPool))]
public class Incinerate : ModRelicTemplate
{
	public override RelicRarity Rarity => RelicRarity.Event;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("IntangibleAmount", 1)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

	public override RelicAssetProfile AssetProfile => new(
		// 小图标（原版85x85）
		IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
		// 轮廓图标（原版85x85）
		IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
		// 大图标（原版256x256）
		BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
	);

	public override Task AfterCardEnteredCombat(CardModel card)
	{
		if (!CanAffect(card))
		{
			return Task.CompletedTask;
		}
		CardCmd.ApplyKeyword(card, CardKeyword.Exhaust);
		return Task.CompletedTask;
	}

	public override Task AfterRoomEntered(AbstractRoom room)
	{
		if (!(room is CombatRoom))
		{
			return Task.CompletedTask;
		}
		IEnumerable<CardModel> allCards = Owner.PlayerCombatState!.AllCards;
		foreach (CardModel item in allCards)
		{
			if (CanAffect(item))
			{
				CardCmd.ApplyKeyword(item, CardKeyword.Exhaust);
			}
		}
		return Task.CompletedTask;
	}

	public bool CanAffect(CardModel card)
	{
		if (card.Rarity == CardRarity.Basic && (card.Tags.Contains(CardTag.Strike) || card.Tags.Contains(CardTag.Defend)))
		{
			return !card.Keywords.Contains(CardKeyword.Exhaust);
		}
		return false;
	}
}
