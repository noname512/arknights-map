using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace ArknightsMap.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class AcceleratorField : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    protected override IEnumerable<DynamicVar> CanonicalVars => [];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [];

    public override RelicAssetProfile AssetProfile =>
        new(
            // 小图标（原版85x85）
            IconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 轮廓图标（原版85x85）
            IconOutlinePath: $"res://ArknightsMap/images/relics/{GetType().Name}.png",
            // 大图标（原版256x256）
            BigIconPath: $"res://ArknightsMap/images/relics/{GetType().Name}.png"
        );

    public static bool DuringEndTurn = false;

    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeSideTurnEnd))]
    public static class BeforeSideTurnEndPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ICombatState combatState, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side != CombatSide.Player)
            {
                DuringEndTurn = true;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(ICombatState combatState, CombatSide side, IEnumerable<Creature> participants)
        {
            DuringEndTurn = false;
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterSideTurnEnd))]
    public static class AfterSideTurnEndPatch
    {
        [HarmonyPrefix]
        public static void Prefix(ICombatState combatState, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side != CombatSide.Player)
            {
                DuringEndTurn = true;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(ICombatState combatState, CombatSide side, IEnumerable<Creature> participants)
        {
            DuringEndTurn = false;
        }
    }

    [HarmonyPatch(typeof(CombatState), nameof(CombatState.IterateHookListeners))]
    public static class IterateHookListenersPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref IEnumerable<AbstractModel> __result)
        {
            __result = DuplicateModels(__result);
        }

        private static IEnumerable<AbstractModel> DuplicateModels(IEnumerable<AbstractModel> original)
        {
            foreach (AbstractModel item in original)
            {
                yield return item;

                if (ShouldDuplicate(item))
                {
                    yield return item;
                }
            }
        }

        private static bool ShouldDuplicate(AbstractModel model)
        {
            Player? owner = null;
            if (model is PowerModel powerModel)
                owner = powerModel.Owner.Player;
            else if (model is RelicModel relicModel)
                owner = relicModel.Owner;
            else if (model is PotionModel potionModel)
                owner = potionModel.Owner;
            else if (model is CardModel cardModel)
                owner = cardModel.Owner;
            else if (model is AfflictionModel afflictionModel)
                owner = afflictionModel.Card.Owner;
            else if (model is EnchantmentModel enchantmentModel)
                owner = enchantmentModel.Card.Owner;
            else if (model is OrbModel orbModel)
                owner = orbModel.Owner;

            if (owner != null && owner.Relics.Any(r => r is AcceleratorField) && DuringEndTurn)
                return true;
            return false;
        }
    }
}
