using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(ReloadableUtility), "FindSomeReloadableComponent")]
internal class patch_ReloadableUtility_FindSomeReloadableComponent
{
    [HarmonyPostfix]
    private static CompReloadable Postfix(CompReloadable __result, Pawn pawn, bool allowForcedReload)
    {
        if (!yayoCombat.ammo || __result != null)
        {
            return __result;
        }

        foreach (var thing in pawn.equipment.AllEquipmentListForReading)
        {
            var compReloadable = thing.TryGetComp<CompReloadable>();
            if (compReloadable?.NeedsReload(allowForcedReload) != true)
            {
                continue;
            }

            __result = compReloadable;
            break;
        }

        return __result;
    }
}