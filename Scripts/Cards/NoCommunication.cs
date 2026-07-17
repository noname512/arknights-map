
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace ArknightsMap.Scripts.Cards
{
    [RegisterCard(typeof(EventCardPool))]
    public class NoCommunication : ModCardTemplate
    {
        private const int energyCost = 5;
        private const CardType type = CardType.Curse;
        private const CardRarity rarity = CardRarity.Curse;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

    

        public NoCommunication() : base(energyCost, type, rarity, targetType)
        {
        }

        public override IEnumerable<CardKeyword> CanonicalKeywords => [
            CardKeyword.Exhaust
        ];

        protected override IEnumerable<DynamicVar> CanonicalVars => [
           
        ];


        protected override IEnumerable<IHoverTip> AdditionalHoverTips
 => [
            
        ];

        

    



    public override CardAssetProfile AssetProfile =>
        new(
            PortraitPath: $"res://ArknightsMap/images/cards/{GetType().Name}.png"
        // 卡框等，有需求自己添加。需要自行判断卡牌类型（攻击、技能、能力等）设置，建议写在基类里。
        // 如果使用自定义卡池，需要改下material（TODO）
        // FramePath: "", // 卡牌背景
        // PortraitBorderPath: "", // 边框（状态牌感染使用的）
        // BannerTexturePath: "" // 横幅（不同类型）
        );

        public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
        {
            if (card == this)
            {
                base.EnergyCost.UpgradeBy(-1);
            }
            return base.AfterCardDrawn(choiceContext, card, fromHandDraw);
        }
        


        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            
        }

        protected override void OnUpgrade()
        {
            
        }
    }
}