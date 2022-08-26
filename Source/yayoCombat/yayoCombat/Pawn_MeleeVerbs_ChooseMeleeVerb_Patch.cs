using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(Pawn_MeleeVerbs), "ChooseMeleeVerb")]
internal class Pawn_MeleeVerbs_ChooseMeleeVerb_Patch
{
    [HarmonyPrefix]
    private static bool Prefix(Pawn_MeleeVerbs __instance, Thing target)
    {
        if (!yayoCombat.advAni)
        {
            return true;
        }

        var updatedAvailableVerbsList = __instance.GetUpdatedAvailableVerbsList(Rand.Chance(0.04f));
        var changeVerb = false;
        if (updatedAvailableVerbsList.TryRandomElementByWeight(ve => ve.GetSelectionWeight(target), out var result))
        {
            changeVerb = true;
        }
        else if (Rand.Chance(0.04f))
        {
            updatedAvailableVerbsList = __instance.GetUpdatedAvailableVerbsList(false);
            changeVerb =
                updatedAvailableVerbsList.TryRandomElementByWeight(ve => ve.GetSelectionWeight(target), out result);
        }

        AccessTools.Method(typeof(Pawn_MeleeVerbs), "SetCurMeleeVerb")
            .Invoke(__instance, changeVerb ? new object[] { result.verb, target } : new object[2]);

        return false;
    }
}