using ArknightsMap.Scripts.Enchantments;
using ArknightsMap.Scripts.Powers;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class Gospel : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MaxHpVar(4)];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public override async Task BeforeRoomEntered(AbstractRoom _)
    {
        if (!Owner.Creature.IsDead)
        {
            MapPoint? currentMapPoint = Owner.RunState.CurrentMapPoint;
            if (
                currentMapPoint != null
                && currentMapPoint.PointType != MapPointType.Monster
                && currentMapPoint.PointType != MapPointType.Elite
                && currentMapPoint.PointType != MapPointType.Boss
            )
            {
                Flash();
                await CreatureCmd.GainMaxHp(Owner.Creature, DynamicVars.MaxHp.IntValue);
                List<CardModel> upgradableCards = PileType.Deck.GetPile(Owner).Cards.Where(c => c is { IsUpgradable: true }).ToList();
                if (upgradableCards.Count > 1)
                {
                    int index = Owner.RunState.Rng.Niche.NextInt(0, upgradableCards.Count - 1);
                    CardCmd.Upgrade(upgradableCards[index], CardPreviewStyle.MessyLayout);
                }
                else if (upgradableCards.Count == 1)
                {
                    CardCmd.Upgrade(upgradableCards[0], CardPreviewStyle.MessyLayout);
                }
            }

        }
    }
}