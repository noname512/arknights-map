using System.Reflection;
using System.Reflection.Emit;
using ArknightsMap.Scripts.Utils;
using ArknightsMap.Scripts.Utils.MerchantEnchantment;
using Godot;
using Godot.Collections;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.PeerInput;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Runs;
using static Godot.Control;

namespace ArknightsMap.Scripts.Patches;

class MerchantEntanchmentPatch
{
    [HarmonyPatch(typeof(RunManager), "InitializeShared")]
    public static class InitializeSharedPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            RunManager __instance,
            INetGameService netService,
            PeerInputSynchronizer inputSynchronizer,
            bool shouldSave,
            DateTimeOffset? dailyTime,
            long startTime,
            long runTime,
            long winTime,
            int numReloads
        )
        {
            var propInfo = typeof(RunManager).GetProperty("State", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            RunState? currentState = propInfo?.GetValue(__instance) as RunState;
            EnchantSynchronizer synchronizer = new EnchantSynchronizer(__instance.RunLocationTargetedBuffer, netService, currentState, netService.NetId);
            ModExtensions.SetEnchantSynchronizer(__instance, synchronizer);
        }
    }

    [HarmonyPatch(typeof(RunManager), "CleanUp")]
    public static class CleanUpPatch
    {
        [HarmonyPostfix]
        public static void Postfix(RunManager __instance, bool graceful)
        {
            __instance.GetEnchantSynchronizer().Dispose();
        }
    }

    [HarmonyPatch(typeof(MerchantInventory), "CreateForNormalMerchant")]
    public static class CreateForNormalMerchantPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Player player, ref MerchantInventory __result)
        {
            List<MerchantEnchantmentEntry> entries = new List<MerchantEnchantmentEntry>();
            MerchantInventory inventory = __result;
            for (int i = 0; i < 6; i++)
            {
                MerchantEnchantmentEntry enchantmentEntry = new MerchantEnchantmentEntry(player);
                enchantmentEntry.PurchaseCompleted += (status, entry) =>
                {
                    var method = typeof(MerchantInventory).GetMethod(
                        "UpdateEntries",
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        [typeof(PurchaseStatus), typeof(MerchantEntry)],
                        null
                    );
                    method?.Invoke(inventory, [status, entry]);
                };
                entries.Add(enchantmentEntry);
            }
            ModExtensions.SetEnchantmentEntries(inventory, entries);
            ModExtensions.SetEntrisBindCount(inventory, 0);
        }
    }

    [HarmonyPatch(typeof(MerchantInventory), "get_AllEntries")]
    public static class GetAllEntriesPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(MerchantInventory __instance, ref IEnumerable<MerchantEntry> __result)
        {
            IEnumerable<MerchantEntry>[] obj = { __instance.CardEntries, __instance.GetEnchantmentEntries(), null };
            IEnumerable<MerchantEntry> enumerable2;
            if (__instance.CardRemovalEntry == null)
            {
                IEnumerable<MerchantEntry> enumerable = [];
                enumerable2 = enumerable;
            }
            else
            {
                IEnumerable<MerchantEntry> enumerable = [__instance.CardRemovalEntry];
                enumerable2 = enumerable;
            }
            obj[2] = enumerable2;
            __result = obj.SelectMany((IEnumerable<MerchantEntry> e) => e);
            return false;
        }
    }

    public static Object? GetField(string fieldName, object __instance, Type? type = null)
    {
        if (type == null)
        {
            type = __instance.GetType();
        }
        FieldInfo? field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field == null)
        {
            GD.PrintErr($"[ArknightsMap] Field {fieldName} not found in {__instance.GetType().Name}!!!");
            return null;
        }
        return field.GetValue(__instance);
    }

    public static IEnumerable<MethodBase> DiscoverMethods(string methodName, bool isProperty = false)
    {
        var targetTypes = new List<Type> { typeof(NMerchantRelic), typeof(NMerchantPotion) };

        foreach (var type in targetTypes)
        {
            MethodInfo? method = null;

            if (isProperty)
            {
                method = type.GetProperty(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetGetMethod(nonPublic: true);
            }
            else
            {
                method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            if (method != null)
                yield return method;
        }
    }

    [HarmonyPatch]
    public static class GetEntryDynamicPatch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods() => DiscoverMethods("Entry", true);

        [HarmonyPrefix]
        public static bool Prefix(NMerchantSlot __instance, ref MerchantEntry __result)
        {
            GD.Print($"====== 动态拦截 Entry 成功！格子真实运行时类型: {__instance.GetType().Name} ======");

            if (__instance is NMerchantRelic || __instance is NMerchantPotion)
            {
                var enchantData = __instance.GetEnchantmentDatas();
                if (enchantData != null && enchantData.entry != null)
                {
                    __result = enchantData.entry;
                    return false;
                }
            }

            return true;
        }
    }

    public static void EnchantmentFillSlot(NMerchantSlot slot)
    {
        GD.Print($"====== Start patching FillSlot ======");
        NMerchantInventory NInventory = (NMerchantInventory)GetField("_merchantRug", slot, typeof(NMerchantSlot));
        MerchantInventory inventory = NInventory.Inventory;
        int id = ModExtensions.GetEntriesBindCount(inventory);
        List<MerchantEnchantmentEntry> list = inventory.GetEnchantmentEntries();
        MerchantEnchantmentEntry entry = list[id];
        entry.EntryUpdated += () => Traverse.Create(slot).Method("UpdateVisual").GetValue();
        entry.PurchaseFailed += (status) =>
        {
            var method = typeof(NMerchantSlot).GetMethod("OnPurchaseFailed", BindingFlags.Instance | BindingFlags.NonPublic);
            method?.Invoke(slot, [status]);
        };
        entry.PurchaseCompleted += (status, entry) =>
        {
            var method = slot.GetType()
                .GetMethod("OnSuccessfulPurchase", BindingFlags.Instance | BindingFlags.NonPublic, null, [typeof(PurchaseStatus), typeof(MerchantEntry)], null);
            method?.Invoke(slot, [status, entry]);
        };
        EnchantmentData data = new EnchantmentData()
        {
            entry = entry,
            model = entry.Model,
            node = null,
        };
        ModExtensions.SetEnchantmentDatas(slot, data);
        ModExtensions.SetEntrisBindCount(inventory, id + 1);
        Traverse.Create(slot).Method("UpdateVisual").GetValue();
        GD.Print($"====== Successfully patched FillSlot, Enchantment: {entry.Model.GetType().Name}");
    }

    [HarmonyPatch(typeof(NMerchantRelic), "FillSlot")]
    public static class RelicFillSlotPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(NMerchantRelic __instance, MerchantRelicEntry relicEntry)
        {
            EnchantmentFillSlot(__instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(NMerchantPotion), "FillSlot")]
    public static class PotionFillSlotPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(NMerchantPotion __instance, MerchantPotionEntry potionEntry)
        {
            EnchantmentFillSlot(__instance);
            return false;
        }
    }

    [HarmonyPatch]
    public static class UpdateVisualDynamicPatch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods() => DiscoverMethods("UpdateVisual", false);

        [HarmonyPrefix]
        public static bool Prefix(NMerchantSlot __instance)
        {
            GD.Print($"====== Start patching UpdateVisual ======");
            EnchantmentData data = __instance.GetEnchantmentDatas();
            MegaLabel costLabel = (MegaLabel)GetField("_costLabel", __instance, typeof(NMerchantSlot));
            if (__instance.Entry.IsStocked)
            {
                costLabel.SetTextAutoSize(__instance.Entry.Cost.ToString());
            }

            if (data.entry.Model == null)
            {
                __instance.Visible = false;
                __instance.MouseFilter = MouseFilterEnum.Ignore;
                if (data.node != null)
                {
                    GodotTreeExtensions.QueueFreeSafely(data.node);
                    data.node = null;
                }
                Traverse.Create(__instance).Method("ClearHoverTip").GetValue();
                return false;
            }
            if (data.node != null && data.node.Model != data.entry.Model)
            {
                GodotTreeExtensions.QueueFreeSafely(data.node);
                data.node = null;
            }
            if (data.node == null)
            {
                data.node = NEnchantment.Create(data.entry.Model);
                Control holder = (Control)GetField(__instance is NMerchantRelic ? "_relicHolder" : "_potionHolder", __instance);
                holder.AddChildSafely(data.node);
                // Holder args copied from PotionHolder
                holder.Scale = new Vector2(1.5f, 1.5f);
                holder.OffsetLeft = -34f;
                holder.OffsetTop = -40f;
                holder.OffsetRight = 6f;
                holder.OffsetBottom = 0f;
                holder.PivotOffset = new Vector2(20f, 20f);
                HBoxContainer costContainer = __instance.GetNode<HBoxContainer>("Cost");
                costContainer.OffsetTop = 48f;
                costContainer.OffsetBottom = 102f;
                costContainer.OffsetLeft = -148f;
                costContainer.OffsetRight = 148f;
                __instance.Hitbox.Size = data.node.Icon!.Size;
                __instance.Hitbox.Scale = holder.Scale;
                __instance.Hitbox.GlobalPosition = data.node.Icon.GlobalPosition;
            }

            costLabel.SetTextAutoSize(data.entry.Cost.ToString());
            costLabel.Modulate = data.entry.EnoughGold ? StsColors.cream : StsColors.red;
            return false;
        }
    }

    [HarmonyPatch]
    public static class OnTryPurchaseDynamicPatch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods() => DiscoverMethods("OnTryPurchase", false);

        [HarmonyPrefix]
        public static bool Prefix(NMerchantSlot __instance, MerchantInventory? inventory, ref Task __result)
        {
            GD.Print($"====== Start patching OnTryPurchase ======");
            __result = __instance.GetEnchantmentDatas().entry.OnTryPurchaseWrapper(inventory);
            return false;
        }
    }

    [HarmonyPatch(typeof(NMerchantRelic), "OnSuccessfulPurchase")]
    [HarmonyPatch(typeof(NMerchantPotion), "OnSuccessfulPurchase")]
    public static class OnSuccessfulPurchasePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(NMerchantSlot __instance)
        {
            GD.Print($"====== Start patching OnSuccessfulPurchase ======");
            Traverse.Create(__instance).Method("TriggerMerchantHandToPointHere").GetValue();
            Traverse.Create(__instance).Method("UpdateVisual").GetValue();
            EnchantmentData data = __instance.GetEnchantmentDatas();
            data.model = data.entry.Model;
            return false;
        }
    }

    [HarmonyPatch]
    public static class CreateHoverTipDynamicPatch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods() => DiscoverMethods("CreateHoverTip", false);

        [HarmonyPrefix]
        public static bool Prefix(NMerchantSlot __instance)
        {
            GD.Print($"====== Start patching CreateHoverTip ======");
            EnchantmentData? data = __instance.GetEnchantmentDatas();
            if (data != null)
                GD.Print("Data Found");
            NEnchantment? node = data.node;
            if (node != null)
                GD.Print("Node Found");
            GD.Print($"model: {node.Model.GetType().Name}");
            NHoverTipSet? nHoverTipSet = NHoverTipSet.CreateAndShow(__instance, data.node.Model.HoverTips);
            nHoverTipSet?.SetGlobalPosition(__instance.GlobalPosition);
            if (nHoverTipSet != null)
            {
                if (__instance.GlobalPosition.X > __instance.GetViewport().GetVisibleRect().Size.X * 0.5f)
                {
                    nHoverTipSet.SetAlignment(__instance, HoverTipAlignment.Left);
                    nHoverTipSet.GlobalPosition -= __instance.Size * 0.5f * __instance.Scale;
                }
                else
                {
                    nHoverTipSet.SetAlignment(__instance, HoverTipAlignment.Right);
                    nHoverTipSet.GlobalPosition +=
                        Vector2.Right * __instance.Size.X * 0.5f * __instance.Scale + Vector2.Up * __instance.Size.Y * 0.5f * __instance.Scale;
                }
            }
            else
            {
                GD.Print("nHoverTipSet is null");
            }
            GD.Print($"====== End patching CreateHoverTip ======");
            return false;
        }
    }

    [HarmonyPatch]
    public static class OnPreviewDynamicPatch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods() => DiscoverMethods("OnPreview", false);

        [HarmonyPrefix]
        public static bool Prefix(NMerchantRelic __instance)
        {
            return false;
        }
    }

    [HarmonyPatch]
    public static class ExitTreeDynamicPatch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods() => DiscoverMethods("_ExitTree", false);

        [HarmonyPrefix]
        public static bool Prefix(NMerchantSlot __instance)
        {
            GD.Print($"====== Start patching _ExitTree ======");
            __instance.Disconnect(
                NMerchantSlot.SignalName.Unhovered,
                Callable.From<NMerchantSlot>((slot) => Traverse.Create(__instance).Method("OnMerchantHandUnhovered").GetValue([slot]))
            );
            var OnUnfocus = () => Traverse.Create(__instance).Method("OnUnfocus").GetValue();
            ((NClickableControl)GetField("_hitbox", __instance)).Disconnect(SignalName.MouseExited, Callable.From(OnUnfocus));
            __instance.Disconnect(SignalName.FocusExited, Callable.From(OnUnfocus));
            ((Tween)GetField("_hoverTween", __instance)).Kill();
            Player player = (Player)GetField("Player", __instance);
            if (player != null)
            {
                player.GoldChanged -= () => Traverse.Create(__instance).Method("UpdateVisual").GetValue();
            }
            MerchantEnchantmentEntry entry = __instance.GetEnchantmentDatas().entry;
            entry.EntryUpdated -= () => Traverse.Create(__instance).Method("UpdateVisual").GetValue();
            entry.PurchaseFailed -= (status) =>
            {
                var method = typeof(NMerchantSlot).GetMethod("OnPurchaseFailed", BindingFlags.Instance | BindingFlags.NonPublic);
                method?.Invoke(__instance, [status]);
            };
            entry.PurchaseCompleted -= (status, entry) => Traverse.Create(__instance).Method("OnSuccessfulPurchase").GetValue([status, entry]);
            return false;
        }
    }
}
