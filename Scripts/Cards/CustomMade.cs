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
using MegaCrit.Sts2.Core.Models.CardPools;


namespace ArknightsMap.Scripts.Cards
{
    [RegisterCard(typeof(EventCardPool))]
    public class CustomMade : ModCardTemplate
    {
        private const int energyCost = 2;
        private const CardType type = CardType.Power;
        private const CardRarity rarity = CardRarity.Event;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

    

        public CustomMade() : base(energyCost, type, rarity, targetType)
        {
        }

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new PowerVar<StrengthPower>(0),
            new PowerVar<DexterityPower>(0),
            new CardsVar(0),
            new BlockVar(0, ValueProp.Move)
            ];


        protected override IEnumerable<IHoverTip> AdditionalHoverTips
 => [
            
        ];    

        


        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext,base.Owner.Creature, base.DynamicVars["StrengthPower"].BaseValue, base.Owner.Creature, this);
            await PowerCmd.Apply<DexterityPower>(choiceContext,base.Owner.Creature, base.DynamicVars["DexterityPower"].BaseValue, base.Owner.Creature, this);
            if (base.DynamicVars["Cards"].BaseValue > 0)
            {
                await CardPileCmd.Draw(choiceContext, base.DynamicVars["Cards"].BaseValue, base.Owner);
            }
            if (base.DynamicVars.Block.BaseValue > 0)
            {
                await CreatureCmd.GainBlock(base.Owner.Creature, DynamicVars.Block, cardPlay);
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }
    }
}