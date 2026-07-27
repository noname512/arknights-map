using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

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

        protected override IEnumerable<IHoverTip> AdditionalHoverTips => [];

        [SavedProperty]
        public int Strength => Relics.CustomMade._strength;

        [SavedProperty]
        public int Dexterity => Relics.CustomMade._dexterity;

        [SavedProperty]
        public int Cards => Relics.CustomMade._cards;

        [SavedProperty]
        public int Blocks => Relics.CustomMade._blocks;

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, Strength, Owner.Creature, this);
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, Dexterity, Owner.Creature, this);
            if (Cards > 0)
            {
                await CardPileCmd.Draw(choiceContext, Cards, Owner);
            }
            if (Blocks > 0)
            {
                await CreatureCmd.GainBlock(Owner.Creature, Blocks, ValueProp.Unpowered, cardPlay);
            }
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
