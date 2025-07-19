using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoCombat.HarmonyPatches;

[HarmonyPatch(typeof(CompApparelReloadable), nameof(CompApparelReloadable.PostExposeData))]
internal class CompApparelReloadable_PostExposeData
{
    private static bool Prefix(CompApparelReloadable __instance, ref int ___remainingCharges)
    {
        if (!yayoCombat.ammo)
        {
            return true;
        }

        if (!__instance.parent.def.IsWeapon)
        {
            return true;
        }

        Scribe_Values.Look(ref ___remainingCharges, "remainingCharges", -999);
        if (Scribe.mode != LoadSaveMode.PostLoadInit || ___remainingCharges != -999)
        {
            return false;
        }

        ___remainingCharges = __instance.MaxCharges;
        return false;
    }
}
