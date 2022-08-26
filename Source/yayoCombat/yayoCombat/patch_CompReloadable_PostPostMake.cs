using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(CompReloadable), "PostPostMake")]
internal class patch_CompReloadable_PostPostMake
{
    [HarmonyPostfix]
    [HarmonyPriority(0)]
    private static void Postfix(CompReloadable __instance, ref int ___remainingCharges)
    {
        if (!yayoCombat.ammo || !__instance.parent.def.IsWeapon)
        {
            return;
        }

        ___remainingCharges = GenTicks.TicksGame <= 5
            ? Mathf.RoundToInt(__instance.MaxCharges * yayoCombat.s_enemyAmmo)
            : 0;
    }
}