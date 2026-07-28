using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Cards;

[RegisterCard(typeof(EventCardPool))]
public class SomethingForm : ModCardTemplate
{
    public override int CanonicalStarCost => baseCard?.CanonicalStarCost ?? 0;
    public override bool HasStarCostX => baseCard?.HasStarCostX ?? false;
    protected override bool HasEnergyCostX => baseCard?.EnergyCost.CostsX ?? false;

    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targetType = TargetType.Self;
    public override CardAssetProfile AssetProfile =>
        new(
            PortraitPath: $"res://ArknightsMap/images/cards/{GetType().Name}.png"
        // 卡框等，有需求自己添加。需要自行判断卡牌类型（攻击、技能、能力等）设置，建议写在基类里。
        // 如果使用自定义卡池，需要改下material（TODO）
        // FramePath: "", // 卡牌背景
        // PortraitBorderPath: "", // 边框（状态牌感染使用的）
        // BannerTexturePath: "" // 横幅（不同类型）
        );

    private CardModel? baseCard;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar("cardName")];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    public SomethingForm()
        : base(0, type, rarity, targetType) { }

    public void SetBaseCard(CardModel baseCard)
    {
        EnergyCost.UpgradeBy(baseCard.EnergyCost.Canonical);
        this.baseCard = baseCard;
        StringVar stringVar = (StringVar)DynamicVars["cardName"];
        stringVar.StringValue = baseCard.Title;
    }

    protected override void OnUpgrade()
    {
        int costBefore = baseCard!.EnergyCost.Canonical;
        CardCmd.Upgrade(baseCard, CardPreviewStyle.None);
        EnergyCost.UpgradeBy(baseCard.EnergyCost.Canonical - costBefore);
    }

    protected override void AfterDowngraded()
    {
        int costBefore = baseCard!.EnergyCost.Canonical;
        CardCmd.Downgrade(baseCard);
        EnergyCost.UpgradeBy(baseCard.EnergyCost.Canonical - costBefore);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        SomethingFormPower power = ModelDb.Power<SomethingFormPower>();
        int amount = 1;
        if (EnergyCost.CostsX)
        {
            amount = ResolveEnergyXValue();
            power._stackType = PowerStackType.Counter;
            power.xType = SomethingFormPower.XType.XEnergy;
        }
        else if (HasStarCostX)
        {
            amount = ResolveStarXValue();
            power._stackType = PowerStackType.Counter;
            power.xType = SomethingFormPower.XType.XStar;
        }
        power.baseCard = baseCard;
        await PowerCmd.Apply(choiceContext, power, Owner.Creature, amount, Owner.Creature, this);
    }
}
