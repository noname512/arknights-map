using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class WildfireSpread : ModRelicTemplate
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

	public override RelicAssetProfile AssetProfile => new(
		// 小图标（原版85x85）
		IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
		// 轮廓图标（原版85x85）
		IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
		// 大图标（原版256x256）
		BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
	);
	private bool _wasUsedThisCombat;
	private bool WasUsedThisCombat
	{
		get
		{
			return _wasUsedThisCombat;
		}
		set
		{
			AssertMutable();
			_wasUsedThisCombat = value;
		}
	}
	public override Task AfterRoomEntered(AbstractRoom room)
	{
		if (!(room is CombatRoom))
		{
			return Task.CompletedTask;
		}
		WasUsedThisCombat = false;
		base.Status = RelicStatus.Active;
		return Task.CompletedTask;
	}

	public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		if (card.Owner == base.Owner && !WasUsedThisCombat && ((card.Type == CardType.Skill) || (card.Type == CardType.Attack)))
		{
			Flash();
			for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
			{
				CardModel card2 = card.CreateClone();
				await CardPileCmd.AddGeneratedCardToCombat(card2, PileType.Hand, Owner);
			}
			Status = RelicStatus.Normal;
			WasUsedThisCombat = true;
		}
	}

	public override Task AfterCombatEnd(CombatRoom _)
	{
		WasUsedThisCombat = false;
		base.Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}
}
