using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Cards;

[RegisterCard(typeof(EventCardPool))]
public class Cargo : ModCardTemplate
{
    private const int energyCost = -1;
    private const CardType type = CardType.Quest;
    private const CardRarity rarity = CardRarity.Quest;
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

    public static HashSet<Player> RemovedInCurRoom = new HashSet<Player>();

    protected override IEnumerable<DynamicVar> CanonicalVars => [new MaxHpVar(30)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    public Cargo()
        : base(energyCost, type, rarity, targetType) { }

    public override Task BeforeRoomEntered(AbstractRoom room)
    {
        RemovedInCurRoom.Clear();
        return Task.CompletedTask;
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (!(room is MerchantRoom))
        {
            return;
        }
        if (RemovedInCurRoom.Contains(Owner))
        {
            return;
        }
        RemovedInCurRoom.Add(Owner);
        PlayerCmd.CompleteQuest(this);
        await CardPileCmd.RemoveFromDeck(this, true);
        await CreatureCmd.GainMaxHp(Owner.Creature, DynamicVars.MaxHp.BaseValue);
    }
}
