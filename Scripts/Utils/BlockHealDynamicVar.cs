using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace ArknightsMap.Scripts.Utils;

public class BlockHealDynamicVar : DynamicVar
{
    public ValueProp Props { get; }

    public BlockHealDynamicVar(decimal block, ValueProp props)
        : base("BlockHeal", block)
    {
        Props = props;
    }

    public BlockHealDynamicVar(string name, decimal block, ValueProp props)
        : base(name, block)
    {
        Props = props;
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        card.DynamicVars.Damage.UpdateCardPreview(card, previewMode, target, runGlobalHooks);
        decimal num = card.DynamicVars.Damage.PreviewValue / 2;
        BaseValue = num;
        EnchantmentModel enchantment = card.Enchantment;
        if (enchantment != null)
        {
            num += enchantment.EnchantBlockAdditive(num);
            num *= enchantment.EnchantBlockMultiplicative(num);
            if (!card.IsEnchantmentPreview)
            {
                EnchantedValue = num;
            }
        }
        if (runGlobalHooks)
        {
            num = Hook.ModifyBlock(card.CombatState, card.Owner.Creature, BaseValue, Props, card, null, out IEnumerable<AbstractModel> _);
        }
        else if (!card.IsEnchantmentPreview)
        {
            if (enchantment != null)
            {
                num += enchantment.EnchantBlockAdditive(num);
                num *= enchantment.EnchantBlockMultiplicative(num);
            }
            PreviewValue = num;
        }
        PreviewValue = num;
    }    
}