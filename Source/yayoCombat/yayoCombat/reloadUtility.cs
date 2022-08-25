using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using JobDefOf = yayoCombat_Defs.JobDefOf;

namespace yayoCombat;

internal class reloadUtility
{
    internal static void EjectAmmo(Pawn pawn, ThingWithComps t)
    {
        if (!pawn.IsColonist && pawn.equipment.Primary == null)
        {
            return;
        }

        var job = new Job(JobDefOf.EjectAmmo, t)
        {
            count = 1
        };
        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
    }

    internal static void EjectAmmoAction(Pawn p, CompReloadable cp)
    {
        var num = 0;
        while (cp.RemainingCharges > 0)
        {
            cp.UsedOnce();
            num++;
        }

        while (num > 0)
        {
            var thing = ThingMaker.MakeThing(cp.AmmoDef);
            thing.stackCount = Mathf.Min(thing.def.stackLimit, num) * cp.Props.ammoCountPerCharge;
            num -= thing.stackCount;
            GenPlace.TryPlaceThing(thing, p.Position, p.Map, ThingPlaceMode.Near);
        }

        cp.Props.soundReload.PlayOneShot(new TargetInfo(p.Position, p.Map));
    }

    internal static void TryThingEjectAmmoDirect(Thing w, bool forbidden = false)
    {
        if (!w.def.IsWeapon || w.TryGetComp<CompReloadable>() == null)
        {
            return;
        }

        var compReloadable = w.TryGetComp<CompReloadable>();
        var num = 0;
        while (compReloadable.RemainingCharges > 0)
        {
            compReloadable.UsedOnce();
            num++;
        }

        while (num > 0)
        {
            var thing = ThingMaker.MakeThing(compReloadable.AmmoDef);
            thing.stackCount = Mathf.Min(thing.def.stackLimit, num) * compReloadable.Props.ammoCountPerCharge;
            if (forbidden)
            {
                thing.SetForbidden(true);
            }

            num -= thing.stackCount;
            GenPlace.TryPlaceThing(thing, w.Position, w.Map, ThingPlaceMode.Near);
        }
    }

    public static Thing getEjectableWeapon(IntVec3 c, Map m)
    {
        foreach (var thing in c.GetThingList(m))
        {
            var compReloadable = thing.TryGetComp<CompReloadable>();
            if (compReloadable is { RemainingCharges: > 0 })
            {
                return thing;
            }
        }

        return null;
    }

    public static void tryAutoReload(CompReloadable cp)
    {
        if (cp.RemainingCharges > 0)
        {
            return;
        }

        var p = cp.Wearer;
        var list = p.inventory.innerContainer.ToList();
        var list2 = new List<Thing>();
        foreach (var thing in list)
        {
            if (thing.def == cp.AmmoDef)
            {
                list2.Add(thing);
            }
        }

        if (list2.Count == 0 && !p.RaceProps.Humanlike && yayoCombat.refillMechAmmo)
        {
            var thing = ThingMaker.MakeThing(cp.AmmoDef);
            thing.stackCount = cp.MaxAmmoNeeded(true);
            p.inventory.innerContainer.TryAdd(thing);
            list2.Add(thing);
        }

        if (list2.Count > 0)
        {
            var list3 = new List<Thing>();
            var num = cp.MaxAmmoNeeded(true);
            for (var num2 = list2.Count - 1; num2 >= 0; num2--)
            {
                var num3 = Mathf.Min(num, list2[num2].stackCount);
                if (!p.inventory.innerContainer.TryDrop(list2[num2], p.Position, p.Map, ThingPlaceMode.Direct, num3,
                        out var resultingThing))
                {
                    p.inventory.innerContainer.TryDrop(list2[num2], p.Position, p.Map, ThingPlaceMode.Near, num3,
                        out resultingThing);
                }

                if (num3 > 0)
                {
                    num -= num3;
                    list3.Add(resultingThing);
                }

                if (num <= 0)
                {
                    break;
                }
            }

            if (list3.Count <= 0)
            {
                return;
            }

            var job = JobMaker.MakeJob(RimWorld.JobDefOf.Reload, cp.parent);
            job.targetQueueB = list3.Select(t => new LocalTargetInfo(t)).ToList();
            job.count = list3.Sum(t => t.stackCount);
            job.count = Math.Min(job.count, cp.MaxAmmoNeeded(true));
            p.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            p.jobs.jobQueue.EnqueueLast(JobMaker.MakeJob(RimWorld.JobDefOf.Goto, p.Position));
        }
        else if (yayoCombat.supplyAmmoDist >= 0)
        {
            var list4 = RefuelWorkGiverUtility.FindEnoughReservableThings(
                desiredQuantity: new IntRange(cp.MinAmmoNeeded(false), cp.MaxAmmoNeeded(false)), pawn: p,
                rootCell: p.Position,
                validThing: t => t.def == cp.AmmoDef && p.Position.DistanceTo(t.Position) <= yayoCombat.supplyAmmoDist);
            if (list4 == null || p.jobs.jobQueue.ToList().Count > 0)
            {
                return;
            }

            p.jobs.TryTakeOrderedJob(JobGiver_Reload.MakeReloadJob(cp, list4), JobTag.Misc);
            p.jobs.jobQueue.EnqueueLast(JobMaker.MakeJob(RimWorld.JobDefOf.Goto, p.Position));
        }
    }
}