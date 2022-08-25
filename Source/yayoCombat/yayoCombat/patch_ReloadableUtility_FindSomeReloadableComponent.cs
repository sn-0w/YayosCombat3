using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(ReloadableUtility), "FindSomeReloadableComponent")]
internal class patch_ReloadableUtility_FindSomeReloadableComponent
{
    [HarmonyPostfix]
    private static bool Prefix(ref CompReloadable __result, Pawn pawn, bool allowForcedReload)
    {
        if (!yayoCombat.ammo)
        {
            return true;
        }

        var allEquipmentListForReading = pawn.equipment.AllEquipmentListForReading;
        foreach (var thingWithComps in allEquipmentListForReading)
        {
            var compReloadable = thingWithComps.TryGetComp<CompReloadable>();
            if (compReloadable == null || !compReloadable.NeedsReload(allowForcedReload))
            {
                continue;
            }

            __result = compReloadable;
            return false;
        }

        return true;
    }
}