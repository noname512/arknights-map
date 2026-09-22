using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class Kd4 : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DexterityPower>(3), new PowerVar<StrengthPower>(1)];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<DexterityPower>(), HoverTipFactory.FromPower<StrengthPower>()];

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource
    )
    {
        if (target == Owner.Creature && dealer != null && (props.IsPoweredAttack() || cardSource is Omnislice))
        {
            Flash();
            await CreatureCmd.Damage(choiceContext, dealer, amount * 2, ValueProp.Unpowered | ValueProp.SkipHurtAnim, Owner.Creature, null, null);
        }
    }

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState
    )
    {
        if (participants.Contains(Owner.Creature))
        {
            Flash();
            await PowerCmd.Apply<StrengthPower>(choiceContext, combatState.Enemies, DynamicVars.Strength.IntValue, Owner.Creature, null);
            if (Owner.PlayerCombatState!.TurnNumber <= 1)
            {
                await PowerCmd.Apply<DexterityPower>(choiceContext, combatState.Enemies, DynamicVars.Dexterity.IntValue, Owner.Creature, null);
            }
        }
    }
}
