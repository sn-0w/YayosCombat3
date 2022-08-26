using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), "GetGizmos")]
internal class patch_ThingWithComps_GetGizmos
{
    [HarmonyPostfix]
    private static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn_EquipmentTracker __instance)
    {
        foreach (var gizmo in __result)
        {
            yield return gizmo;
        }

        if (!yayoCombat.ammo)
        {
            yield break;
        }

        if (!PawnAttackGizmoUtility.CanShowEquipmentGizmos())
        {
            yield break;
        }

        foreach (var thing in __instance.AllEquipmentListForReading)
        {
            foreach (var comp in thing.AllComps)
            {
                foreach (var gizmo in comp.CompGetWornGizmosExtra())
                {
                    yield return gizmo;
                }
            }
        }
    }
}