using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(ReloadableUtility), "FindPotentiallyReloadableGear")]
internal class patch_ReloadableUtility_FindPotentiallyReloadableGear
{
    [HarmonyPostfix]
    private static bool Prefix(ref IEnumerable<Pair<CompReloadable, Thing>> __result, Pawn pawn,
        List<Thing> potentialAmmo)
    {
        if (!yayoCombat.ammo)
        {
            return true;
        }

        var list = new List<Pair<CompReloadable, Thing>>();
        if (pawn.apparel != null)
        {
            var wornApparel = pawn.apparel.WornApparel;
            foreach (var apparel in wornApparel)
            {
                var compReloadable = apparel.TryGetComp<CompReloadable>();
                if (compReloadable?.AmmoDef == null)
                {
                    continue;
                }

                foreach (var thing in potentialAmmo)
                {
                    if (thing.def == compReloadable.Props.ammoDef)
                    {
                        list.Add(new Pair<CompReloadable, Thing>(compReloadable, thing));
                    }
                }
            }
        }

        if (pawn.equipment != null)
        {
            var allEquipmentListForReading = pawn.equipment.AllEquipmentListForReading;
            foreach (var thingWithComps in allEquipmentListForReading)
            {
                var compReloadable2 = thingWithComps.TryGetComp<CompReloadable>();
                if (compReloadable2?.AmmoDef == null)
                {
                    continue;
                }

                foreach (var thing2 in potentialAmmo)
                {
                    if (thing2.def == compReloadable2.Props.ammoDef)
                    {
                        list.Add(new Pair<CompReloadable, Thing>(compReloadable2, thing2));
                    }
                }
            }
        }

        __result = list;
        return false;
    }
}