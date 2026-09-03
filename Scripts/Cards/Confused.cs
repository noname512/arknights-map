using ArknightsMap.Scripts.Monsters;
using ArknightsMap.Scripts.Powers;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Cards;

[RegisterCard(typeof(CurseCardPool))]
public class Confused : ModCardTemplate
{
    public override int MaxUpgradeLevel => 0;

    // 基础耗能
    private const int energyCost = -1;

    // 卡牌类型
    private const CardType type = CardType.Curse;

    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Curse;

    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.None;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromCard<Confused>(), HoverTipFactory.FromKeyword(ConfusedKeyword.Keyword)];

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

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Eternal];

    public Confused()
        : base(energyCost, type, rarity, targetType) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) { }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay
    )
    {
        if (target?.Side != CombatSide.Enemy)
        {
            return 1m;
        }

        if (!props.IsPoweredAttack())
        {
            return 1m;
        }
        if (dealer != Owner.Creature)
        {
            return 1m;
        }
        if (!Owner.PlayerCombatState!.Hand.Cards.Contains(this))
        {
            return 1m;
        }
        return 0.5m;
    }

    public override bool HasTurnEndInHandEffect => true;

    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        bool alreadyHasFrail = Owner.Creature.HasPower<ConfusedPower>();
        foreach (Creature c in CombatState!.HittableEnemies)
        {
            if (c.Monster is not SupersweetieSmiley || c.Monster is not TheSaint || c.Monster is not OpForGun)
            {
                await PowerCmd.Apply<ConfusedPower>(choiceContext, c, 1, null, this);
            }
        }
    }

    protected override bool IsPlayable => false;

    // 升级后的效果逻辑
    protected override void OnUpgrade() { }
}
