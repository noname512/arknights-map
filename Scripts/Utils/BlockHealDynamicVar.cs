using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
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
        decimal num;
        if (card.DynamicVars.TryGetValue("Damage", out DynamicVar damage))
        {
            damage.UpdateCardPreview(card, previewMode, target, runGlobalHooks);
            num = damage.PreviewValue / 2;
        }
        else if (card.DynamicVars.TryGetValue("CalculatedDamage", out DynamicVar calcDamage))
        {
            calcDamage.UpdateCardPreview(card, previewMode, target, runGlobalHooks);
            num = calcDamage.PreviewValue / 2;
        }
        else
        {
            num = 0;        // 放弃思考，显示bug就显示bug吧
        }

        BaseValue = num;
        EnchantmentModel? enchantment = card.Enchantment;
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
            num = Hook.ModifyBlock(card.CombatState!, card.Owner.Creature, BaseValue, Props, card, null, out IEnumerable<AbstractModel> _);
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