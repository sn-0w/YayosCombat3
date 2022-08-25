using RimWorld;
using UnityEngine;
using Verse;

namespace yayoCombat;

public static class ArmorUtility
{
    public const float MaxArmorRating = 2f;

    public const float DeflectThresholdFactor = 0.5f;

    public static float GetPostArmorDamage(Pawn pawn, float amount, float armorPenetration, BodyPartRecord part,
        ref DamageDef damageDef, ref bool deflectedByMetalArmor, ref bool diminishedByMetalArmor, DamageInfo dinfo)
    {
        deflectedByMetalArmor = false;
        diminishedByMetalArmor = false;
        bool forcedDefl;
        if (damageDef.armorCategory == null)
        {
            return amount;
        }

        var armorRatingStat = damageDef.armorCategory.armorRatingStat;
        if (pawn.apparel != null)
        {
            var wornApparel = pawn.apparel.WornApparel;
            for (var num = wornApparel.Count - 1; num >= 0; num--)
            {
                var apparel = wornApparel[num];
                if (!apparel.def.apparel.CoversBodyPart(part))
                {
                    continue;
                }

                var num2 = amount;
                ApplyArmor(ref amount, armorPenetration, apparel.GetStatValue(armorRatingStat), apparel,
                    ref damageDef, pawn, out var metalArmor, dinfo, out forcedDefl);
                if (amount < 0.001f)
                {
                    deflectedByMetalArmor = metalArmor || forcedDefl;

                    return 0f;
                }

                if (amount < num2 && metalArmor)
                {
                    diminishedByMetalArmor = true;
                }
            }
        }

        var num3 = amount;
        ApplyArmor(ref amount, armorPenetration, pawn.GetStatValue(armorRatingStat), null, ref damageDef, pawn,
            out var metalArmor2, dinfo, out forcedDefl);
        if (amount < 0.001f)
        {
            deflectedByMetalArmor = metalArmor2 || forcedDefl;

            return 0f;
        }

        if (amount < num3 && metalArmor2)
        {
            diminishedByMetalArmor = true;
        }

        if (forcedDefl)
        {
            deflectedByMetalArmor = true;
        }

        return amount;
    }

    public static void ApplyArmor(ref float damAmount, float armorPenetration, float armorRating, Thing armorThing,
        ref DamageDef damageDef, Pawn pawn, out bool metalArmor, DamageInfo dinfo, out bool forcedDefl)
    {
        var isArmor = false;
        var isMechanoid = pawn.RaceProps.IsMechanoid;
        forcedDefl = false;
        if (armorThing != null)
        {
            isArmor = true;
            metalArmor = armorThing.def.apparel.useDeflectMetalEffect ||
                         armorThing.Stuff is { IsMetal: true };
        }
        else
        {
            metalArmor = isMechanoid;
        }

        var num = armorPenetration;
        if (isArmor && dinfo.Weapon != null)
        {
            if ((int)armorThing.def.techLevel >= 5 || isMechanoid)
            {
                if (dinfo.Weapon.IsMeleeWeapon)
                {
                    if ((int)dinfo.Weapon.techLevel <= 3)
                    {
                        num *= 0.5f;
                    }
                }
                else if ((int)dinfo.Weapon.techLevel <= 3)
                {
                    num *= 0.35f;
                }
            }
            else if ((int)armorThing.def.techLevel >= 4)
            {
                if (dinfo.Weapon.IsMeleeWeapon)
                {
                    if ((int)dinfo.Weapon.techLevel <= 2)
                    {
                        num *= 0.75f;
                    }
                }
                else if ((int)dinfo.Weapon.techLevel <= 3)
                {
                    num *= 0.5f;
                }
            }
        }

        var num2 = Mathf.Max(armorRating - num, 0f);
        var value = (num - (armorRating * 0.15f)) * 5f;
        value = Mathf.Clamp01(value);
        var value2 = Rand.Value;
        if (isArmor)
        {
            var f = damAmount * (0.2f + (value * 0.5f));
            armorThing.TakeDamage(new DamageInfo(damageDef, GenMath.RoundRandom(f)));
        }

        var num4 = !isArmor
            ? pawn.health.summaryHealth.SummaryHealthPercent
            : armorThing.HitPoints / (float)armorThing.MaxHitPoints;
        var num5 = Mathf.Max((armorRating * 0.9f) - num, 0f);
        var num6 = 1f - yayoCombat.s_armorEf;
        if (value2 * num6 < num5 * num4)
        {
            if (Rand.Value < Mathf.Min(num2, 0.9f))
            {
                damAmount = 0f;
            }
            else if (isArmor)
            {
                forcedDefl = true;
                damAmount = GenMath.RoundRandom(damAmount * (0.25f + (value * 0.25f)));
                if (damageDef.armorCategory == DamageArmorCategoryDefOf.Sharp)
                {
                    damageDef = DamageDefOf.Blunt;
                }
            }
            else
            {
                forcedDefl = true;
                damAmount = GenMath.RoundRandom(damAmount * (0.25f + (value * 0.5f)));
                if (damageDef.armorCategory == DamageArmorCategoryDefOf.Sharp)
                {
                    damageDef = DamageDefOf.Blunt;
                }
            }
        }
        else if (value2 < num2 * (0.5f + (num4 * 0.5f)))
        {
            damAmount = GenMath.RoundRandom(damAmount * 0.5f);
            if (damageDef.armorCategory == DamageArmorCategoryDefOf.Sharp)
            {
                damageDef = DamageDefOf.Blunt;
            }
        }
    }
}