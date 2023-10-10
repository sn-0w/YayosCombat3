using HarmonyLib;
using UnityEngine;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(Thing), "TakeDamage")]
public static class Thing_TakeDamage_Patch
{
    [HarmonyPrefix]
    private static void Prefix(ref DamageInfo dinfo)
    {
        if (!yayoCombat.advAni)
        {
            return;
        }

        if (!(dinfo.Amount > 0f))
        {
            return;
        }

        if (dinfo.Weapon is not { IsMeleeWeapon: true })
        {
            return;
        }

        var amount = Mathf.Max(1f, Mathf.RoundToInt(dinfo.Amount * yayoCombat.meleeDelay));
        dinfo.SetAmount(amount);
    }
}