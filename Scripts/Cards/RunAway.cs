using ArknightsMap.Scripts.Powers;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Cards;

[RegisterCard(typeof(TokenCardPool))]
public class RunAway : ModCardTemplate, KnowledgeDemon.IChoosable
{
    public override int MaxUpgradeLevel => 0;
    public override bool CanBeGeneratedInCombat => false;

    // 基础耗能
    private const int energyCost = -1;

    // 卡牌类型
    private const CardType type = CardType.Status;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Status;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.None;

    // 卡图资源
    public override CardAssetProfile AssetProfile =>
        new(
            PortraitPath: $"res://ArknightsMap/images/cards/{GetType().Name}.png"
        // 卡框等，有需求自己添加。需要自行判断卡牌类型（攻击、技能、能力等）设置，建议写在基类里。
        // 如果使用自定义卡池，需要改下material（TODO）
        // FramePath: "", // 卡牌背景
        // PortraitBorderPath: "", // 边框（状态牌感染使用的）
        // BannerTexturePath: "" // 横幅（不同类型）
        );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            // 阻挡相关描述
        ];

    public RunAway()
        : base(energyCost, type, rarity, targetType) { }

    public async Task OnChosen()
    {
        await CreaturePositions.MoveTo(Owner.Creature, 2);
    }
}
