using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(DrugPolicy), "ExposeData")]
internal class Patch_DrugPolicy
{
    private static readonly AccessTools.FieldRef<DrugPolicy, List<DrugPolicyEntry>> s_entriesInt =
        AccessTools.FieldRefAccess<DrugPolicy, List<DrugPolicyEntry>>("entriesInt");

    [HarmonyPriority(1000)]
    private static bool Prefix(DrugPolicy __instance)
    {
        if (!yayoCombat.ammo)
        {
            return true;
        }

        if (s_entriesInt(__instance) == null)
        {
            s_entriesInt(__instance) = new List<DrugPolicyEntry>();
        }

        Scribe_Values.Look(ref __instance.uniqueId, "uniqueId");
        Scribe_Values.Look(ref __instance.label, "label");
        Scribe_Collections.Look(ref s_entriesInt(__instance), "drugs", LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit && s_entriesInt(__instance) != null &&
            s_entriesInt(__instance).RemoveAll(x => x == null || x.drug == null) != 0)
        {
            Log.Error("Some DrugPolicyEntries were null after loading.");
        }

        return false;
    }
}