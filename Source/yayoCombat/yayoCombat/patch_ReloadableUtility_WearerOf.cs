using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(ReloadableUtility), "WearerOf")]
internal class patch_ReloadableUtility_WearerOf
{
    [HarmonyPostfix]
    private static Pawn Postfix(Pawn __result, CompReloadable comp)
    {
        if (!yayoCombat.ammo || __result != null)
        {
            return __result;
        }

        if (comp.ParentHolder is Pawn_EquipmentTracker equipmentTracker)
        {
            __result = equipmentTracker.pawn;
        }
        // could also check "is Pawn_InventoryTracker inventoryTracker", might cause problems though?

        return __result;
    }
}