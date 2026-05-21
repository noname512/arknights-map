using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArknightsMap.Scripts.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class LockedBreechBurst : ModRelicTemplate
{
	public override RelicRarity Rarity => RelicRarity.Ancient;
	
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromCard<OverloadStrike>()];
	
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
		List<CardPileAddResult> results = new List<CardPileAddResult>();
		CardModel card = Owner.RunState.CreateCard<OverloadStrike>(Owner);
		results.Add(await CardPileCmd.Add(card, PileType.Deck));
		CardCmd.PreviewCardPileAdd(results, 1f);
	}
}
