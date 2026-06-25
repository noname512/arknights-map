using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class SnowTracks : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Rooms", 3), new CardsVar(4), new HealVar(30)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    private int _snowTracksIndex = -1;

    [SavedProperty]
    public int SnowTracksIndex
    {
        get { return _snowTracksIndex; }
        set
        {
            AssertMutable();
            _snowTracksIndex = value;
        }
    }

    [SavedProperty]
    public int[] SnowTracksCoordCols = [];

    [SavedProperty]
    public int[] SnowTracksCoordRows = [];

    public override Task AfterObtained()
    {
        SnowTracksIndex = Owner.RunState.CurrentActIndex;
        AddMarkedRooms(Owner.RunState.Map);
        return Task.CompletedTask;
    }

    public override ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
    {
        return AddMarkedRooms(map);
    }

    private ActMap AddMarkedRooms(ActMap map)
    {
        if (base.Owner.RunState.CurrentActIndex != SnowTracksIndex)
        {
            return map;
        }
        List<MapCoord> markedCoords = GetMarkedCoords();
        if (!markedCoords.Any())
        {
            Rng rng = new Rng((uint)((int)Owner.RunState.Rng.Seed + (int)Owner.NetId + StringHelper.GetDeterministicHashCode(GetType().Name)));
            List<MapPoint> list = map.GetAllMapPoints()
                .Where(
                    delegate(MapPoint p)
                    {
                        MapPointType pointType = p.PointType;
                        return (int)pointType <= 6 && !p.Quests.Any((AbstractModel q) => q is SnowTracks);
                    }
                )
                .ToList();
            list.UnstableShuffle(rng);
            list = list.Take(DynamicVars["Rooms"].IntValue).ToList();
            SnowTracksCoordCols = list.Select(l => l.coord.col).ToArray();
            SnowTracksCoordRows = list.Select(l => l.coord.row).ToArray();
            foreach (MapPoint item in list)
            {
                item.AddQuest(this);
            }
        }
        else
        {
            foreach (MapCoord item in markedCoords)
            {
                MapPoint? point = map.GetPoint(item);
                if (point == null)
                {
                    throw new InvalidOperationException(
                        $"Loaded a snow tracks map with coordinate {item}, but the generated map does not contain that coordinate!"
                    );
                }
                point.AddQuest(this);
            }
        }
        return map;
    }

    public override async Task BeforeRoomEntered(AbstractRoom room)
    {
        List<MapCoord> markedCoords = GetMarkedCoords();
        if (!markedCoords.Contains(Owner.RunState.CurrentMapPoint!.coord))
        {
            return;
        }
        Flash();
        Player player = LocalContext.GetMe(Owner.RunState.Players)!;
        IEnumerable<CardModel> enumerable = PileType
            .Deck.GetPile(player)
            .Cards.Where((CardModel c) => c?.IsUpgradable ?? false)
            .ToList()
            .StableShuffle(player.RunState.Rng.Niche)
            .Take(DynamicVars.Cards.IntValue);
        NRun.Instance?.GlobalUi.GridCardPreviewContainer.ForceMaxColumnsUntilEmpty(3);
        foreach (CardModel item in enumerable)
        {
            CardCmd.Upgrade(item, CardPreviewStyle.GridLayout);
        }
        await CreatureCmd.Heal(player.Creature, DynamicVars.Heal.IntValue, false);
    }

    public List<MapCoord> GetMarkedCoords()
    {
        return SnowTracksCoordCols.Zip(SnowTracksCoordRows, (c, r) => new MapCoord { col = c, row = r }).ToList();
    }
}
