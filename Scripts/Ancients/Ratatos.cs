using ArknightsMap.Scripts.Acts;
using ArknightsMap.Scripts.Relics;
using Godot;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Ancients;

[RegisterSharedAncient]
public class Ratatos : ModAncientEventTemplate
{
    // 选项按钮颜色
    public override Color ButtonColor => new(0.46f, 0.26f, 0.18f, 0.5f);

    // 对话框颜色
    public override Color DialogueColor => new(0.58f, 0.33f, 0.23f);

    // 自定义场景的路径
    public override EventAssetProfile AssetProfile => new(BackgroundScenePath: $"res://ArknightsMap/scenes/ancients/{GetType().Name}.tscn");

    // 自定义地图图标和轮廓的路径
    public override AncientEventPresentationAssetProfile AncientPresentationAssetProfile =>
        new(
            MapIconPath: "res://icon.svg", //$"res://ArknightsMap/images/ancients/{GetType().Name}/icon.png",
            MapIconOutlinePath: "res://icon.svg", //$"res://ArknightsMap/images/ancients/{GetType().Name}/icon_outline.png",
            RunHistoryIconPath: $"res://ArknightsMap/images/ancients/{GetType().Name}/avatar.png",
            RunHistoryIconOutlinePath: $"res://ArknightsMap/images/ancients/{GetType().Name}/avatar.png"
        );

    private IEnumerable<EventOption> Pool1Single =>
        [
            CreateModRelicOption<EmptyShelves>(), // 空置货架
            CreateModRelicOption<LongTermContract>(), // 长期契约
        ];
    private IEnumerable<EventOption> Pool2 =>
        [
            CreateModRelicOption<BurningHouse>(), // 燃烧屋
            CreateModRelicOption<CooperationAgreement>(), // 合作协议
            CreateModRelicOption<Reinforcement>(), // 外援
            CreateModRelicOption<LoanSharking>(), // 高利贷
        ];
    private IEnumerable<EventOption> Pool3 =>
        [
            CreateModRelicOption<TradeMembershipCard>(), // 贸易会员卡
            CreateModRelicOption<SnowyRealmShop>(), // 雪境专卖店
            CreateModRelicOption<TradeShippingOrder>(), // 货运委托书
        ];

    // 所有可能的选项
    public override IEnumerable<EventOption> AllPossibleOptions => [.. Pool1Single, .. Pool2, .. Pool3, CreateModRelicOption<TriClanCouncil>()];

    // 生成选项
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        List<EventOption> Pool1 = Pool1Single.ToList();
        if (Owner!.RunState.Players.Count > 1)
        {
            Pool1.Add(RelicOption<TriClanCouncil>());
        }
        return [Rng.NextItem(Pool1)!, Rng.NextItem(Pool2)!, Rng.NextItem(Pool3)!];
    }

    public override bool IsValidForAct(ActModel act)
    {
        return act is SnowyMountain;
    }
}
