using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Intents;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace ArknightsMap.Scripts.Patches;

class IgniteIntentPatch
{
    [HarmonyPatch(typeof(IntentAnimData), nameof(IntentAnimData.GetAnimationFrame))]
    public static class GetAnimationFramePatch
    {
        public static bool Prefix(string animation, int frame, ref string __result)
        {
            if (animation == "arknights_map_intent_ignite")
            {
                __result = "res://ArknightsMap/images/util/IgniteIntent.tres";
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(NIntent), "UpdateVisuals")]
    public static class UpdateVisualsPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var targetMethod = AccessTools.Method(typeof(IntentAnimData), nameof(IntentAnimData.GetAnimationFrameCount));
            var proxyMethod = AccessTools.Method(typeof(UpdateVisualsPatch), nameof(NewGetCount));

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(targetMethod))
                {
                    yield return new CodeInstruction(OpCodes.Call, proxyMethod);
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        public static int NewGetCount(string animation)
        {
            if (animation == "arknights_map_intent_ignite")
                return 1;
            return IntentAnimData.GetAnimationFrameCount(animation);
        }
    }
}
