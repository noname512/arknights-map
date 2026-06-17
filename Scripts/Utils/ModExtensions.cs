using System.Runtime.CompilerServices;
using ArknightsMap.Scripts.Utils.MerchantEnchantment;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Runs;

namespace ArknightsMap.Scripts.Utils;

public class EnchantmentData
{
    public MerchantEnchantmentEntry entry;
    public EnchantmentModel model;
    public NEnchantment node;
}

public static class ModExtensions
{
    private static readonly ConditionalWeakTable<RunManager, EnchantSynchronizer> EnchantSynchronizers = new();

    private static readonly ConditionalWeakTable<MerchantInventory, List<MerchantEnchantmentEntry>> EnchantmentEntries = new();
    private static readonly Dictionary<MerchantInventory, int> EntriesBindCount = new();

    private static readonly ConditionalWeakTable<NMerchantSlot, EnchantmentData> EnchantmentDatas = new();

    public static EnchantSynchronizer GetEnchantSynchronizer(this RunManager runManager)
    {
        EnchantSynchronizers.TryGetValue(runManager, out var data);
        return data;
    }

    public static List<MerchantEnchantmentEntry> GetEnchantmentEntries(this MerchantInventory inventory)
    {
        EnchantmentEntries.TryGetValue(inventory, out var data);
        return data ?? [];
    }

    public static int GetEntriesBindCount(MerchantInventory inventory)
    {
        return EntriesBindCount[inventory];
    }

    public static EnchantmentData GetEnchantmentDatas(this NMerchantSlot slot)
    {
        EnchantmentDatas.TryGetValue(slot, out var data);
        return data;
    }

    public static void SetEnchantSynchronizer(RunManager runManager, EnchantSynchronizer synchronizer)
    {
        EnchantSynchronizers.Add(runManager, synchronizer);
    }

    public static void SetEnchantmentEntries(MerchantInventory inventory, List<MerchantEnchantmentEntry> enchantmentEntry)
    {
        EnchantmentEntries.Add(inventory, enchantmentEntry);
    }

    public static void SetEntrisBindCount(MerchantInventory inventory, int num)
    {
        EntriesBindCount[inventory] = num;
    }

    public static void SetEnchantmentDatas(NMerchantSlot slot, EnchantmentData data)
    {
        EnchantmentDatas.Add(slot, data);
    }

    public static void RemoveEnchantSynchronizer(RunManager runManager)
    {
        EnchantSynchronizers.Remove(runManager);
    }
}
