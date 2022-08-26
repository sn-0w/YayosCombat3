using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(ReloadableUtility), "FindPotentiallyReloadableGear")]
internal class patch_ReloadableUtility_FindPotentiallyReloadableGear
{
    [HarmonyPostfix]
    private static IEnumerable<Pair<CompReloadable, Thing>> Postfix(IEnumerable<Pair<CompReloadable, Thing>> __result,
        Pawn pawn, List<Thing> potentialAmmo)
    {
        foreach (var pair in __result)
        {
            yield return pair;
        }

        if (!yayoCombat.ammo)
        {
            yield break;
        }

        if (pawn.equipment == null)
        {
            yield break;
        }

        foreach (var thing in pawn.equipment.AllEquipmentListForReading)
        {
            var comp = thing.TryGetComp<CompReloadable>();
            if (comp?.AmmoDef == null)
            {
                continue;
            }

            foreach (var ammoThing in potentialAmmo)
            {
                if (ammoThing?.def == comp.Props.ammoDef)
                {
                    yield return new Pair<CompReloadable, Thing>(comp, ammoThing);
                }
            }
        }
    }
}