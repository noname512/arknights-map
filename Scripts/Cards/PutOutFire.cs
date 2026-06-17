using ArknightsMap.Scripts.Powers;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Cards;

[RegisterCard(typeof(TokenCardPool))]
public class PutOutFire : ModCardTemplate
{
    public override int MaxUpgradeLevel => 0;

    // 基础耗能
    private const int energyCost = 0;

    // 卡牌类型
    private const CardType type = CardType.Status;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Status;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.Self;

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

    static Func<CardModel?, Creature?, decimal> CalculateDamage = (cardModel, creature) =>
    {
        decimal damage = cardModel!.DynamicVars["PutOutDmg"].BaseValue;
        foreach (var m in cardModel.Owner.Creature?.CombatState?.Enemies ?? [])
            damage += m.GetPowerAmount<ScorchingLightPower>();
        return damage;
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => [ModCardVars.Computed("PutOutDmg", 10, CalculateDamage)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal, CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [
            HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
            HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
            HoverTipFactory.FromKeyword(ReedBedKeyword.Keyword),
            HoverTipFactory.FromPower<FlamingDamagePower>(),
            HoverTipFactory.FromPower<VulnerablePower>(),
        ];

    public PutOutFire()
        : base(energyCost, type, rarity, targetType) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(Owner.Creature));
        SfxCmd.Play("event:/sfx/characters/attack_fire");
        decimal amount = DynamicVars["PutOutDmg"].PreviewValue;
        await CreatureCmd.Damage(choiceContext, Owner.Creature, new DamageVar(amount, ValueProp.Unpowered), this);
        await PowerCmd.Apply<FlamingDamagePower>(choiceContext, Owner.Creature, amount, Owner.Creature, this);
        await Entry.reedBed.SetBurningDurningCombat(false, CombatState!);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade() { }
}
