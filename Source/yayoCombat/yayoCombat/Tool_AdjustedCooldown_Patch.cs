using HarmonyLib;
using UnityEngine;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(Tool), "AdjustedCooldown", typeof(Thing))]
internal class Tool_AdjustedCooldown_Patch
{
    [HarmonyPriority(0)]
    private static void Postfix(ref float __result, Thing ownerEquipment)
    {
        if (!yayoCombat.advAni || ownerEquipment == null || !(__result > 0f) ||
            ownerEquipment.ParentHolder is not { ParentHolder: Pawn } ||
            ownerEquipment.def is not { IsMeleeWeapon: true })
        {
            return;
        }

        var num = yayoCombat.meleeDelay * (1f + ((Rand.Value - 0.5f) * yayoCombat.meleeRandom));
        __result = Mathf.Max(__result * num, 0.2f);
    }
}