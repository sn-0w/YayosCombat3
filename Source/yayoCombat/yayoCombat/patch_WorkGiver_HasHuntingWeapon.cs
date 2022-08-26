using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(WorkGiver_HunterHunt), "HasHuntingWeapon")]
internal class patch_WorkGiver_HasHuntingWeapon
{
    [HarmonyPostfix]
    private static bool Postfix(bool __result, Pawn p)
    {
        if (!yayoCombat.ammo || !__result)
        {
            return __result;
        }

        var comp = p.equipment.Primary.GetComp<CompReloadable>();
        if (comp != null)
        {
            __result = comp.CanBeUsed;
        }

        return __result;
    }
}