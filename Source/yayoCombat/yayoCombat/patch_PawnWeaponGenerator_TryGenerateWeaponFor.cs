using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(PawnWeaponGenerator), "TryGenerateWeaponFor")]
internal class patch_PawnWeaponGenerator_TryGenerateWeaponFor
{
    private static readonly FieldInfo f_allWeaponPairs =
        AccessTools.Field(typeof(PawnWeaponGenerator), "allWeaponPairs");

    private static readonly AccessTools.FieldRef<List<ThingStuffPair>> s_allWeaponPairs =
        AccessTools.StaticFieldRefAccess<List<ThingStuffPair>>(f_allWeaponPairs);

    private static readonly FieldInfo f_workingWeapons =
        AccessTools.Field(typeof(PawnWeaponGenerator), "workingWeapons");

    private static readonly AccessTools.FieldRef<List<ThingStuffPair>> s_workingWeapons =
        AccessTools.StaticFieldRefAccess<List<ThingStuffPair>>(f_workingWeapons);

    [HarmonyPriority(0)]
    private static bool Prefix(Pawn pawn, PawnGenerationRequest request)
    {
        if (!yayoCombat.ammo)
        {
            return true;
        }

        var list = s_allWeaponPairs();
        var list2 = s_workingWeapons();
        list2.Clear();
        if (pawn.kindDef.weaponTags == null || pawn.kindDef.weaponTags.Count == 0 || !pawn.RaceProps.ToolUser ||
            !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) ||
            pawn.WorkTagIsDisabled(WorkTags.Violent))
        {
            return false;
        }

        var randomInRange = pawn.kindDef.weaponMoney.RandomInRange;
        foreach (var w2 in list)
        {
            var w3 = w2;
            if (w2.Price <= (double)randomInRange &&
                (pawn.kindDef.weaponTags == null ||
                 pawn.kindDef.weaponTags.Any(tag => w3.thing.weaponTags.Contains(tag))) &&
                (w2.thing.generateAllowChance >= 1.0 || Rand.ChanceSeeded(w2.thing.generateAllowChance,
                    pawn.thingIDNumber ^ w2.thing.shortHash ^ 0x1B3B648)))
            {
                list2.Add(w2);
            }
        }

        if (list2.Count == 0)
        {
            return false;
        }

        pawn.equipment.DestroyAllEquipment();
        if (list2.TryRandomElementByWeight(w => w.Commonality * w.Price, out var result))
        {
            var thingWithComps = (ThingWithComps)ThingMaker.MakeThing(result.thing, result.stuff);
            foreach (var item in thingWithComps.AllComps.Where(comp => comp is CompReloadable))
            {
                if (item is not CompReloadable compReloadable)
                {
                    continue;
                }

                if (pawn.Faction is { IsPlayer: true })
                {
                    Traverse.Create(compReloadable).Field("remainingCharges").SetValue(
                        Mathf.Min(compReloadable.MaxCharges,
                            Mathf.RoundToInt(compReloadable.MaxCharges * yayoCombat.s_enemyAmmo *
                                             Rand.Range(0.7f, 1.3f))));
                }
                else if (yayoCombat.s_enemyAmmo <= 1f)
                {
                    Traverse.Create(compReloadable).Field("remainingCharges").SetValue(
                        Mathf.Min(compReloadable.MaxCharges,
                            Mathf.RoundToInt(compReloadable.MaxCharges * yayoCombat.s_enemyAmmo *
                                             Rand.Range(0.7f, 1.3f))));
                }
                else
                {
                    Traverse.Create(compReloadable).Field("remainingCharges").SetValue(
                        Mathf.RoundToInt(compReloadable.MaxCharges * yayoCombat.s_enemyAmmo *
                                         Rand.Range(0.7f, 1.3f)));
                }
            }

            PawnGenerator.PostProcessGeneratedGear(thingWithComps, pawn);
            if (Rand.Value < (request.BiocodeWeaponChance > 0.0
                    ? request.BiocodeWeaponChance
                    : (double)pawn.kindDef.biocodeWeaponChance))
            {
                thingWithComps.TryGetComp<CompBiocodable>()?.CodeFor(pawn);
            }

            pawn.equipment.AddEquipment(thingWithComps);
        }

        list2.Clear();
        return false;
    }
}