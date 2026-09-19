using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Cards;

[RegisterCard(typeof(TokenCardPool))]
public class EmergencyHeater : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    public override int MaxUpgradeLevel => 0;

    public EmergencyHeater()
        : base(energyCost, type, rarity, targetType) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("Amount", 10)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Retain];

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner.Creature)
        {
            return 0m;
        }
        if (dealer == null || dealer.Side != CombatSide.Enemy)
        {
            return 0m;
        }
        if (!props.IsPoweredAttack())
        {
            return 0m;
        }
        if (Pile?.Type != PileType.Hand)
        {
            return 0m;
        }
        return -DynamicVars["Amount"].IntValue;
    }
}
