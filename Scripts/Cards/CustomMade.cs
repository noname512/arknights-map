using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;
using ArknightsMap.Scripts.Utils;
using MegaCrit.Sts2.Core.Models;


namespace ArknightsMap.Scripts.Cards
{
    [RegisterCard(typeof(EventCardPool))]
    public class CustomMade : ModCardTemplate
    {
        private const int energyCost = 1;
        private const CardType type = CardType.Power;
        private const CardRarity rarity = CardRarity.Event;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        public CustomMade()
            : base(energyCost, type, rarity, targetType) { }

        protected override IEnumerable<DynamicVar> CanonicalVars =>
            [new PowerVar<StrengthPower>(0), new PowerVar<DexterityPower>(0), new CardsVar(0), new BlockVar(0, ValueProp.Move)];

        public CustomMade() : base(energyCost, type, rarity, targetType)
        {
        }

        public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
        {
            if (card == this)
            {
                var relic = base.Owner.GetRelic<Scripts.Relics.CustomMade>();    
                base.DynamicVars["StrengthPower"].BaseValue = relic?.Strength ?? 0;
                base.DynamicVars["DexterityPower"].BaseValue = relic?.Dexterity ?? 0;
                base.DynamicVars["Cards"].BaseValue = relic?.Cards ?? 0;
                base.DynamicVars["Block"].BaseValue = relic?.Block ?? 0;
            }
            return base.AfterCardChangedPiles(card, oldPileType, clonedBy);
        }
        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<StrengthPower>(0),
            new PowerVar<DexterityPower>(0),
            new CardsVar(0),
            new BlockVar(0, ValueProp.Move)
            ];


        protected override IEnumerable<IHoverTip> AdditionalHoverTips
 => [
            HoverTipFactory.FromKeyword(CustomKeyword.Keyword)
        ];    


        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext,base.Owner.Creature, DynamicVars["StrengthPower"].BaseValue, base.Owner.Creature, this);
            await PowerCmd.Apply<DexterityPower>(choiceContext,base.Owner.Creature, DynamicVars["DexterityPower"].BaseValue, base.Owner.Creature, this);
            if (DynamicVars["Cards"].BaseValue > 0)
            {
                await CardPileCmd.Draw(choiceContext, DynamicVars["Cards"].BaseValue, base.Owner);
            }
            if (DynamicVars.Block.BaseValue > 0)
            {
                await CreatureCmd.GainBlock(base.Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Unpowered, cardPlay);
            }
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
