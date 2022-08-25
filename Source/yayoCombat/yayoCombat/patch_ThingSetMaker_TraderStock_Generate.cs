using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(ThingSetMaker_TraderStock), "Generate")]
internal class patch_ThingSetMaker_TraderStock_Generate
{
    [HarmonyPostfix]
    private static bool Prefix(ThingSetMaker_TraderStock __instance, ThingSetMakerParams parms, List<Thing> outThings)
    {
        if (!yayoCombat.ammo)
        {
            return true;
        }

        var hasRangedWeapons = false;
        var traderKindDef = parms.traderDef ?? DefDatabase<TraderKindDef>.AllDefsListForReading.RandomElement();
        if (traderKindDef is { defName: "Empire_Caravan_TributeCollector" })
        {
            return true;
        }

        var makingFaction = parms.makingFaction;
        var forTile = parms.tile ?? (Find.AnyPlayerHomeMap != null ? Find.AnyPlayerHomeMap.Tile :
            Find.CurrentMap == null ? -1 : Find.CurrentMap.Tile);
        foreach (var stockGenerator in traderKindDef.stockGenerators)
        {
            if (stockGenerator is StockGenerator_WeaponsRanged)
            {
                hasRangedWeapons = true;
            }

            foreach (var item in stockGenerator.GenerateThings(forTile, parms.makingFaction))
            {
                if (!item.def.tradeability.TraderCanSell())
                {
                    Log.Error(traderKindDef + " generated carrying " + item +
                              " which can't be sold by traders. Ignoring...");
                    continue;
                }

                item.PostGeneratedForTrader(traderKindDef, forTile, makingFaction);
                outThings.Add(item);
            }
        }

        if (!hasRangedWeapons && !(Rand.Value <= 0.2f))
        {
            return false;
        }

        var techLevel = TechLevel.Spacer;
        if (makingFaction is { def: { } })
        {
            techLevel = makingFaction.def.techLevel;
        }

        var num = 300f;
        var min = 0.4f;
        var max = 1.6f;
        if ((int)techLevel < 2)
        {
            return false;
        }

        Thing thing;
        if ((int)techLevel >= 2 && (int)techLevel <= 3 || Rand.Value <= 0.3f)
        {
            thing = ThingMaker.MakeThing(ThingDef.Named("yy_ammo_primitive"));
            thing.stackCount = Mathf.RoundToInt(Rand.Range(min, max) * yayoCombat.ammoGen * num);
            thing.PostGeneratedForTrader(traderKindDef, forTile, makingFaction);
            outThings.Add(thing);
        }

        if ((int)techLevel >= 4 || Rand.Value <= 0.2f)
        {
            thing = ThingMaker.MakeThing(ThingDef.Named("yy_ammo_industrial"));
            thing.stackCount = Mathf.RoundToInt(Rand.Range(min, max) * yayoCombat.ammoGen * num);
            thing.PostGeneratedForTrader(traderKindDef, forTile, makingFaction);
            outThings.Add(thing);
        }

        if ((int)techLevel >= 4 || Rand.Value <= 0.2f)
        {
            thing = ThingMaker.MakeThing(ThingDef.Named("yy_ammo_industrial_fire"));
            thing.stackCount = Mathf.RoundToInt(Rand.Range(min, max) * yayoCombat.ammoGen * num * 0.5f);
            thing.PostGeneratedForTrader(traderKindDef, forTile, makingFaction);
            outThings.Add(thing);
        }

        if ((int)techLevel >= 4 || Rand.Value <= 0.2f)
        {
            thing = ThingMaker.MakeThing(ThingDef.Named("yy_ammo_industrial_emp"));
            thing.stackCount = Mathf.RoundToInt(Rand.Range(min, max) * yayoCombat.ammoGen * num * 0.25f);
            thing.PostGeneratedForTrader(traderKindDef, forTile, makingFaction);
            outThings.Add(thing);
        }

        if ((int)techLevel >= 5 || Rand.Value <= 0.2f)
        {
            thing = ThingMaker.MakeThing(ThingDef.Named("yy_ammo_spacer"));
            thing.stackCount = Mathf.RoundToInt(Rand.Range(min, max) * yayoCombat.ammoGen * num);
            thing.PostGeneratedForTrader(traderKindDef, forTile, makingFaction);
            outThings.Add(thing);
        }

        if ((int)techLevel >= 5 || Rand.Value <= 0.2f)
        {
            thing = ThingMaker.MakeThing(ThingDef.Named("yy_ammo_spacer_fire"));
            thing.stackCount = Mathf.RoundToInt(Rand.Range(min, max) * yayoCombat.ammoGen * num * 0.5f);
            thing.PostGeneratedForTrader(traderKindDef, forTile, makingFaction);
            outThings.Add(thing);
        }

        if ((int)techLevel >= 5 || Rand.Value <= 0.2f)
        {
            thing = ThingMaker.MakeThing(ThingDef.Named("yy_ammo_spacer_emp"));
            thing.stackCount = Mathf.RoundToInt(Rand.Range(min, max) * yayoCombat.ammoGen * num * 0.25f);
            thing.PostGeneratedForTrader(traderKindDef, forTile, makingFaction);
            outThings.Add(thing);
        }

        return false;
    }
}