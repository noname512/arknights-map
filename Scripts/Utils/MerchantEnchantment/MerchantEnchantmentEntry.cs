using ArknightsMap.Scripts.Enchantments;
using Godot;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Runs;

namespace ArknightsMap.Scripts.Utils.MerchantEnchantment;

public sealed class MerchantEnchantmentEntry : MerchantEntry
{
    private readonly IEnumerable<EnchantmentModel> ValidEnchantments =
    [
        ModelDb.Enchantment<Sharp>(),
        ModelDb.Enchantment<Nimble>(),
        ModelDb.Enchantment<Swift>(),
        ModelDb.Enchantment<Steady>(),
        ModelDb.Enchantment<SpiralSpecial>(),
        ModelDb.Enchantment<Glam>(),
        ModelDb.Enchantment<Vigorous>(),
    ];

    public EnchantmentModel? Model;

    public MerchantEnchantmentEntry(Player player)
        : base(player)
    {
        FillSlot();
    }

    public override bool IsStocked => Model != null;

    public override void CalcCost()
    {
        _cost = EnchantmentMerchantUtils.GetBaseCost(Model!);
        _cost = Mathf.RoundToInt(_cost * 1.5 * _player.PlayerRng.Shops.NextFloat(0.9f, 1.1f));
    }

    protected override void ClearAfterPurchase()
    {
        Model = null;
    }

    protected override async Task<(bool, int)> OnTryPurchase(MerchantInventory? inventory, bool ignoreCost)
    {
        GD.Print("OnTryPurchase");
        int goldToSpend = ignoreCost ? 0 : Cost;
        bool flag = await RunManager.Instance.GetEnchantSynchronizer().DoLocalEnchant(goldToSpend, Model!, true);
        return (flag, goldToSpend);
    }

    protected override void RestockAfterPurchase(MerchantInventory? inventory)
    {
        FillSlot();
    }

    private void FillSlot()
    {
        Model = EnchantmentMerchantUtils.GenerateModel(_player, null);
        CalcCost();
    }
}
