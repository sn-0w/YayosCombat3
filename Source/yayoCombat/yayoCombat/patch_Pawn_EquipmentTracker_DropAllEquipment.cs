using HarmonyLib;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), "DropAllEquipment")]
internal class patch_Pawn_EquipmentTracker_DropAllEquipment
{
    [HarmonyPostfix]
    private static bool Prefix(Pawn_EquipmentTracker __instance, ThingOwner<ThingWithComps> ___equipment, IntVec3 pos,
        bool forbid = true)
    {
        if (!yayoCombat.ammo)
        {
            return true;
        }

        for (var num = ___equipment.Count - 1; num >= 0; num--)
        {
            var thingWithComps = ___equipment[num];
            if (__instance.TryDropEquipment(thingWithComps, out var _, pos, forbid) &&
                __instance.pawn.Faction is { IsPlayer: false })
            {
                reloadUtility.TryThingEjectAmmoDirect(thingWithComps, true);
            }
        }

        return false;
    }
}