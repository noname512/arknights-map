using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Cards;

[RegisterCard(typeof(StatusCardPool))]
public class Cold : ModCardTemplate
{
    private const int energyCost = -1;
    private const CardType type = CardType.Status;
    private const CardRarity rarity = CardRarity.Status;
    private const TargetType targetType = TargetType.Self;
    public override int MaxUpgradeLevel => 0;
    public override CardAssetProfile AssetProfile =>
        new(
            PortraitPath: $"res://ArknightsMap/images/cards/{GetType().Name}.png"
        // 卡框等，有需求自己添加。需要自行判断卡牌类型（攻击、技能、能力等）设置，建议写在基类里。
        // 如果使用自定义卡池，需要改下material（TODO）
        // FramePath: "", // 卡牌背景
        // PortraitBorderPath: "", // 边框（状态牌感染使用的）
        // BannerTexturePath: "" // 横幅（不同类型）
        );

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Ethereal];

    public Cold()
        : base(energyCost, type, rarity, targetType) { }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        return TryModifyCost(card, originalCost, out modifiedCost);
    }

    public override bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        return TryModifyCost(card, originalCost, out modifiedCost);
    }

    private bool TryModifyCost(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!CombatManager.Instance.IsInProgress)
        {
            return false;
        }
        if (card.Owner != Owner)
        {
            return false;
        }
        if (card.Pile?.Type == PileType.Hand || card.Pile?.Type == PileType.Play)
        {
            modifiedCost += 1;
            return true;
        }
        return false;
    }
}
