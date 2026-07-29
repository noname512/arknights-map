using System.Reflection;
using ArknightsMap.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Cards;

[RegisterCard(typeof(EventCardPool))]
public class SomethingForm : ModCardTemplate
{
    public override int CanonicalStarCost => Card?.CanonicalStarCost ?? 0;
    public override bool HasStarCostX => Card?.HasStarCostX ?? false;
    protected override bool HasEnergyCostX => Card?.EnergyCost.CostsX ?? false;
    public override int MaxUpgradeLevel => Card?.MaxUpgradeLevel ?? 0;

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

    private SerializableCard? _serializableCard;

    [SavedProperty]
    public SerializableCard? BaseCard
    {
        get { return _serializableCard; }
        private set
        {
            AssertMutable();
            _serializableCard = value;
            _card = null;
            UpdateCard();
        }
    }

    private CardModel? _card;

    public CardModel? Card
    {
        get
        {
            if (_card == null && BaseCard != null)
            {
                _card = FromSerializable(BaseCard);
            }
            return _card;
        }
    }

    public override string Title
    {
        get
        {
            LocString title = new("cards", Id.Entry + ".title");
            DynamicVars.AddTo(title);
            return title.GetFormattedText();
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar("CardName")];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => _extraHoverTips;

    private List<IHoverTip> _extraHoverTips = new List<IHoverTip>();

    public SomethingForm()
        : base(0, type, rarity, targetType) { }

    public void SetBaseCard(CardModel baseCard)
    {
        BaseCard = baseCard.ToSerializable();
        ForceSetUpgradeLevel(baseCard.CurrentUpgradeLevel);
    }

    private void ForceSetUpgradeLevel(int level)
    {
        FieldInfo currentUpgradeLevelField = typeof(CardModel).GetField("_currentUpgradeLevel", BindingFlags.Instance | BindingFlags.NonPublic)!;
        currentUpgradeLevelField.SetValue(this, level);
    }

    private void UpdateCard()
    {
        _extraHoverTips = [];
        if (Card != null)
        {
            EnergyCost.SetCustomBaseCost(Card.EnergyCost.Canonical);
            _extraHoverTips.AddRange(Card.HoverTips);
            _extraHoverTips.Add(HoverTipFactory.FromCard(Card));
            ((StringVar)DynamicVars["CardName"]).StringValue = Card.Title;
        }
    }

    protected override void OnUpgrade()
    {
        if (BaseCard != null)
        {
            CardModel upgradedCard = FromSerializable(BaseCard);
            if (upgradedCard.IsUpgradable)
            {
                CardCmd.Upgrade(upgradedCard, CardPreviewStyle.None);
            }
            SetBaseCard(upgradedCard);
        }
    }

    protected override void AfterDowngraded()
    {
        if (BaseCard != null)
        {
            CardModel downgradedCard = FromSerializable(BaseCard);
            if (downgradedCard.IsUpgraded)
            {
                CardCmd.Downgrade(downgradedCard);
            }
            SetBaseCard(downgradedCard);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        SomethingFormPower power = (SomethingFormPower)ModelDb.Power<SomethingFormPower>().ToMutable();
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
        if (Card!.Owner == null)
        {
            Card.Owner = Owner;
        }
        power.SetBaseCard(Card);
        await PowerCmd.Apply(choiceContext, power, Owner.Creature, amount, Owner.Creature, this);
    }
}
