using ArknightsMap.Scripts.Relics;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Cards;

[RegisterCard(typeof(EventCardPool))]
public class CustomMadeCard : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targetType = TargetType.Self;

    public CustomMadeCard()
        : base(energyCost, type, rarity, targetType) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<StrengthPower>(0), new PowerVar<DexterityPower>(0), new CardsVar(0), new BlockVar(0, ValueProp.Move)];

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

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card == this)
        {
            var relic = Owner.GetRelic<CustomMade>();
            DynamicVars["StrengthPower"].BaseValue = relic?.Strength ?? 0;
            DynamicVars["DexterityPower"].BaseValue = relic?.Dexterity ?? 0;
            DynamicVars["Cards"].BaseValue = relic?.Cards ?? 0;
            DynamicVars["Block"].BaseValue = relic?.Block ?? 0;
        }
        return Task.CompletedTask;
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [StaticTips.Custom];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars["StrengthPower"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, DynamicVars["DexterityPower"].BaseValue, Owner.Creature, this);
        if (DynamicVars["Cards"].BaseValue > 0)
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars["Cards"].BaseValue, Owner);
        }
        if (DynamicVars.Block.BaseValue > 0)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Unpowered, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
