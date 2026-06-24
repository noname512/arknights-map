using ArknightsMap.Scripts.Enchantments;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace ArknightsMap.Scripts.Utils.MerchantEnchantment;

class EnchantmentMerchantUtils
{
    public static readonly IEnumerable<EnchantmentModel> ValidEnchantments =
    [
        ModelDb.Enchantment<Sharp>(),
        ModelDb.Enchantment<Nimble>(),
        ModelDb.Enchantment<Swift>(),
        ModelDb.Enchantment<Steady>(),
        ModelDb.Enchantment<SpiralSpecial>(),
        ModelDb.Enchantment<Glam>(),
        ModelDb.Enchantment<Vigorous>(),
    ];

    public static EnchantmentModel GenerateModel(Player player, CardModel? card)
    {
        EnchantmentModel model = player.PlayerRng.Shops.NextItem(ValidEnchantments.Where(e => card == null ? true : e.CanEnchant(card)))!.ToMutable();
        if (model is Sharp || model is Nimble)
        {
            model.Amount = player.PlayerRng.Shops.NextInt(1, 10);
        }
        else if (model is Swift)
        {
            model.Amount = player.PlayerRng.Shops.NextInt(1, 3);
        }
        else if (model is Vigorous)
        {
            model.Amount = player.PlayerRng.Shops.NextInt(4, 20);
        }
        return model;
    }

    public static int GetBaseCost(EnchantmentModel model)
    {
        int cost = 0;
        switch (model)
        {
            case Sharp:
            case Nimble:
                cost = model.Amount * 12;
                break;
            case Swift:
                cost = model.Amount * 40;
                break;
            case Steady:
                cost = 50;
                break;
            case SpiralSpecial:
                cost = 130;
                break;
            case Glam:
                cost = 100;
                break;
            case Vigorous:
                cost = model.Amount * 6;
                break;
        }
        return cost;
    }
}
