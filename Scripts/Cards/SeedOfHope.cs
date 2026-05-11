using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Cards;

[RegisterCard(typeof(QuestCardPool))]
public class SeedOfHope : ModCardTemplate
{
    public override int MaxUpgradeLevel => 0;
    // 基础耗能
    private const int energyCost = -1;
    // 卡牌类型
    private const CardType type = CardType.Quest;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Quest;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;
    // 卡图资源
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://ArknightsMap/images/cards/{GetType().Name}.png"
    // 卡框等，有需求自己添加。需要自行判断卡牌类型（攻击、技能、能力等）设置，建议写在基类里。
    // 如果使用自定义卡池，需要改下material（TODO）
    // FramePath: "", // 卡牌背景
    // PortraitBorderPath: "", // 边框（状态牌感染使用的）
    // BannerTexturePath: "" // 横幅（不同类型）
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(15)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    public SeedOfHope() : base(energyCost, type, rarity, targetType)
    {
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (Pile.Type == PileType.Exhaust)
        {
            return Task.CompletedTask;
        }

        int HpLose = Owner.Creature.MaxHp - Owner.Creature.CurrentHp;
        int HealAmount = (int)(0.15F * HpLose);
        CreatureCmd.Heal(Owner.Creature, HealAmount);
        return Task.CompletedTask;
    }
    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
    }
}