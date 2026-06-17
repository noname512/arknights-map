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
        ModelDb.Enchantment<Spiral>(),
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
        switch (Model)
        {
            case Sharp:
            case Nimble:
                _cost = Model.Amount * 12;
                break;
            case Swift:
                _cost = Model.Amount * 40;
                break;
            case Steady:
                _cost = 50;
                break;
            case Spiral:
                _cost = 200;
                break;
            case Glam:
                _cost = 150;
                break;
            case Vigorous:
                _cost = Model.Amount * 6;
                break;
        }
        _cost = Mathf.RoundToInt(_cost * _player.PlayerRng.Shops.NextFloat(0.9f, 1.1f));
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
        Model = _player.PlayerRng.Shops.NextItem(ValidEnchantments).ToMutable();
        if (Model is Sharp || Model is Nimble)
        {
            Model.Amount = _player.PlayerRng.Shops.NextInt(1, 10);
        }
        else if (Model is Swift)
        {
            Model.Amount = _player.PlayerRng.Shops.NextInt(1, 3);
        }
        else if (Model is Vigorous)
        {
            Model.Amount = _player.PlayerRng.Shops.NextInt(4, 20);
        }
        CalcCost();
    }
}
