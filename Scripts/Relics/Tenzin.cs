using ArknightsMap.Scripts.Cards;
using ArknightsMap.Scripts.Enchantments;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class Tenzin : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromEnchantment<UseOnce>();

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (!Owner.Creature.IsDead)
        {
            MapPoint? currentMapPoint = Owner.RunState.CurrentMapPoint;
            if (currentMapPoint != null && currentMapPoint.PointType == MapPointType.Unknown && Owner.RunState.CurrentRoomCount <= 1)
            {
                Flash();
                List<CardPileAddResult> results = new List<CardPileAddResult>();
                CardModel card = Owner.RunState.CreateCard<Scount>(Owner);
                CardCmd.Enchant<UseOnce>(card, 1);
                results.Add(await CardPileCmd.Add(card, PileType.Deck));
                CardCmd.PreviewCardPileAdd(results, 1f);
            }
        }
    }
}
