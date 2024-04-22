using HarmonyLib;
using RimWorld;
using RimWorld.Utility;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(ReloadableUtility), nameof(ReloadableUtility.OwnerOf))]
internal class patch_ReloadableUtility_WearerOf
{
    [HarmonyPostfix]
    private static Pawn Postfix(Pawn __result, IReloadableComp reloadable)
    {
        if (!yayoCombat.ammo || __result != null)
        {
            return __result;
        }

        if (reloadable is CompApparelReloadable { ParentHolder: Pawn_EquipmentTracker equipmentTracker })
        {
            __result = equipmentTracker.pawn;
        }
        // could also check "is Pawn_InventoryTracker inventoryTracker", might cause problems though?

        return __result;
    }
}