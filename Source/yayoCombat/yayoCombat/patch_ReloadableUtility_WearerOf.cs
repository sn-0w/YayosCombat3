using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(ReloadableUtility), "WearerOf")]
internal class patch_ReloadableUtility_WearerOf
{
    [HarmonyPostfix]
    private static bool Prefix(ref Pawn __result, CompReloadable comp)
    {
        if (!yayoCombat.ammo)
        {
            return true;
        }

        __result = comp.ParentHolder is Pawn_EquipmentTracker pawn_EquipmentTracker ? pawn_EquipmentTracker.pawn :
            comp.ParentHolder is Pawn_ApparelTracker pawn_ApparelTracker ? pawn_ApparelTracker.pawn : null;
        return false;
    }
}