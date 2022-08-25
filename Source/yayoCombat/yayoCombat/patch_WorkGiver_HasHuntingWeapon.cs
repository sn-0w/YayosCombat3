using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(WorkGiver_HunterHunt), "HasHuntingWeapon")]
internal class patch_WorkGiver_HasHuntingWeapon
{
    [HarmonyPostfix]
    private static bool Prefix(ref bool __result, Pawn p)
    {
        if (!yayoCombat.ammo)
        {
            return true;
        }

        if (p.equipment.Primary != null && p.equipment.Primary.def.IsRangedWeapon &&
            p.equipment.PrimaryEq.PrimaryVerb.HarmsHealth() &&
            !p.equipment.PrimaryEq.PrimaryVerb.UsesExplosiveProjectiles())
        {
            __result = p.equipment.Primary.GetComp<CompReloadable>() == null ||
                       p.equipment.Primary.GetComp<CompReloadable>().CanBeUsed;
        }
        else
        {
            __result = false;
        }

        return false;
    }
}