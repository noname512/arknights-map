using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Cards;

[RegisterCard(typeof(EventCardPool))]
public class CallForAssistance : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Ancient;
    private const TargetType targetType = TargetType.AnyEnemy;
    public override CardAssetProfile AssetProfile =>
        new(
            PortraitPath: $"res://ArknightsMap/images/cards/{GetType().Name}.png"
        // 卡框等，有需求自己添加。需要自行判断卡牌类型（攻击、技能、能力等）设置，建议写在基类里。
        // 如果使用自定义卡池，需要改下material（TODO）
        // FramePath: "", // 卡牌背景
        // PortraitBorderPath: "", // 边框（状态牌感染使用的）
        // BannerTexturePath: "" // 横幅（不同类型）
        );

    protected override IEnumerable<DynamicVar> CanonicalVars => [new GoldVar(50), new DamageVar(20, ValueProp.Move), new PowerVar<WeakPower>(4)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<WeakPower>(), HoverTipFactory.FromPower<VulnerablePower>()];

    public CallForAssistance()
        : base(energyCost, type, rarity, targetType) { }

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        if (card != this)
        {
            return true;
        }
        if (autoPlayType != AutoPlayType.None)
        {
            return true;
        }
        return Owner.Gold >= DynamicVars.Gold.BaseValue;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.LoseGold(DynamicVars.Gold.BaseValue, Owner, GoldLossType.Spent);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target!).Execute(choiceContext);
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target!, DynamicVars.Weak.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target!, DynamicVars.Weak.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
        DynamicVars.Weak.UpgradeValueBy(1);
    }

    [HarmonyPatch]
    public static class GetPlayerDialogueLinePatch
    {
        static MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("MegaCrit.Sts2.Core.Entities.Cards.UnplayableReasonExtensions");
            return AccessTools.Method(type, "GetPlayerDialogueLine");
        }

        [HarmonyPrefix]
        public static bool Prefix(AbstractModel? preventer, ref LocString? __result)
        {
            if (preventer is CallForAssistance)
            {
                __result = new LocString("combat_messages", "ARKNIGHTS_MAP_NOT_ENOUGH_GOLDS");
                return false;
            }
            return true;
        }
    }
}
