using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class StarrySkyPhoto : ModRelicTemplate
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Rooms", 5)];
	private int roomsEntered = 0;

	public override RelicAssetProfile AssetProfile => new(
		// 小图标（原版85x85）
		IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
		// 轮廓图标（原版85x85）
		IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
		// 大图标（原版256x256）
		BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
	);

	public override async Task AfterRoomEntered(AbstractRoom _)
	{
		if (!base.Owner.Creature.IsDead)
		{
			MapPoint? currentMapPoint = base.Owner.RunState.CurrentMapPoint;
			if (currentMapPoint != null && currentMapPoint.PointType != MapPointType.Monster && currentMapPoint.PointType != MapPointType.Elite && currentMapPoint.PointType != MapPointType.Boss)
			{
				Flash();
				roomsEntered++;
				if (roomsEntered >= DynamicVars["Rooms"].IntValue)
				{
					roomsEntered = 0;
					foreach (var type in new[] { CardType.Attack, CardType.Skill, CardType.Power })
					{
						List<CardModel> upgradableCards = PileType.Deck.GetPile(Owner).Cards.Where(c => c is { IsUpgradable: true } && c.Type == type).ToList();
						if (upgradableCards.Count > 0)
						{
							int index = Owner.RunState.Rng.Niche.NextInt(0, upgradableCards.Count - 1);
							CardCmd.Upgrade(upgradableCards[index], CardPreviewStyle.MessyLayout);
						}
					}
				}
			}
		}
	}
}
