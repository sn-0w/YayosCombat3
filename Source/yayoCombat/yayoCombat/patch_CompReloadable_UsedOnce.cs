using HarmonyLib;
using RimWorld;

namespace yayoCombat;

[HarmonyPatch(typeof(CompReloadable), "UsedOnce")]
internal class patch_CompReloadable_UsedOnce
{
    [HarmonyPostfix]
    private static bool Prefix(CompReloadable __instance, ref int ___remainingCharges)
    {
        if (!yayoCombat.ammo)
        {
            return true;
        }

        if (___remainingCharges > 0)
        {
            ___remainingCharges--;
        }

        if (__instance.Props.destroyOnEmpty && __instance.RemainingCharges == 0 && !__instance.parent.Destroyed)
        {
            __instance.parent.Destroy();
        }

        if (__instance.Wearer == null)
        {
            return false;
        }

        if (___remainingCharges == 0 && __instance.Wearer.CurJobDef == JobDefOf.Hunt)
        {
            __instance.Wearer.jobs.StopAll();
        }

        reloadUtility.tryAutoReload(__instance);
        return false;
    }
}