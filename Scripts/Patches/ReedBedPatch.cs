using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

class ReedBedPatch
{
    // [HarmonyPatch(typeof(CombatRoom), nameof(CombatRoom.EnterInternal))]
    // public class LoadReedBed
    // {
    //     public static void Postfix(CombatRoom _inst, IRunState? runState, bool isRestoringRoomStackBase)
    //     {
    //         if (_inst.Encounter is MyAbstractEncounter)
    //         {
    //         }
    //     }
    // }
}