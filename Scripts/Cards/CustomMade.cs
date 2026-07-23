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
using MegaCrit.Sts2.Core.Saves.Runs;


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

            [SavedProperty]
        public int Strength => Scripts.Relics.CustomMade._strength;

        [SavedProperty]
        public int Dexterity => Scripts.Relics.CustomMade._dexterity;

        [SavedProperty]
        public int Cards => Scripts.Relics.CustomMade._cards;

        [SavedProperty]
        public int Blocks => Scripts.Relics.CustomMade._blocks;



        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext,base.Owner.Creature, Strength, base.Owner.Creature, this);
            await PowerCmd.Apply<DexterityPower>(choiceContext,base.Owner.Creature, Dexterity, base.Owner.Creature, this);
            if (Cards > 0)
            {
                await CardPileCmd.Draw(choiceContext, Cards, base.Owner);
            }
            if (Blocks > 0)
            {
                await CreatureCmd.GainBlock(base.Owner.Creature, Blocks, ValueProp.Unpowered, cardPlay);
            }
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }
    }
}