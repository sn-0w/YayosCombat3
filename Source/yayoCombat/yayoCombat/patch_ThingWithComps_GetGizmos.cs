using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), "GetGizmos")]
internal class patch_ThingWithComps_GetGizmos
{
    [HarmonyPostfix]
    private static bool Prefix(ref IEnumerable<Gizmo> __result, Pawn_EquipmentTracker __instance)
    {
        if (!yayoCombat.ammo)
        {
            return true;
        }

        var list = new List<Gizmo>();
        if (PawnAttackGizmoUtility.CanShowEquipmentGizmos())
        {
            var allEquipmentListForReading = __instance.AllEquipmentListForReading;
            for (var i = 0; i < allEquipmentListForReading.Count; i++)
            {
                foreach (var verbsCommand in allEquipmentListForReading[i].GetComp<CompEquippable>().GetVerbsCommands())
                {
                    switch (i)
                    {
                        case 0:
                            verbsCommand.hotKey = KeyBindingDefOf.Misc1;
                            break;
                        case 1:
                            verbsCommand.hotKey = KeyBindingDefOf.Misc2;
                            break;
                        case 2:
                            verbsCommand.hotKey = KeyBindingDefOf.Misc3;
                            break;
                    }

                    list.Add(verbsCommand);
                }
            }

            foreach (var thingWithComps in allEquipmentListForReading)
            {
                var list2 = thingWithComps.AllComps;
                foreach (var thingComp in list2)
                {
                    foreach (var item in thingComp.CompGetWornGizmosExtra())
                    {
                        list.Add(item);
                    }
                }
            }
        }

        __result = list;
        return false;
    }
}