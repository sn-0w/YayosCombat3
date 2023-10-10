using System.Reflection;
using DualWield.Harmony;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(PawnRenderer), "DrawEquipment")]
internal class patch_DrawEquipment
{
    [HarmonyPrefix]
    private static bool Prefix(PawnRenderer __instance, Vector3 rootLoc, Pawn ___pawn)
    {
        if (!yayoCombat.advAni)
        {
            return true;
        }

        if (yayoCombat.using_meleeAnimations && ___pawn.equipment?.Primary?.def?.IsMeleeWeapon == true)
        {
            return true;
        }

        var rootLoc2 = rootLoc;
        if (___pawn.Dead || !___pawn.Spawned)
        {
            return false;
        }

        if (___pawn.equipment?.Primary == null)
        {
            return false;
        }

        if (___pawn.CurJob != null && ___pawn.CurJob.def.neverShowWeapon)
        {
            return false;
        }

        var y = 0.0005f;
        var num = 0f;
        var num2 = 0.1f;
        ThingWithComps thingWithComps = null;
        var stance_Busy = ___pawn.stances.curStance as Stance_Busy;
        Stance_Busy stance_Busy2 = null;
        var offsetMainHand = Vector3.zero;
        var offsetOffHand = Vector3.zero;
        LocalTargetInfo localTargetInfo = null;
        var num3 = 143f;
        if (stance_Busy is { neverAimWeapon: false })
        {
            localTargetInfo = stance_Busy.focusTarg;
            if (localTargetInfo != null)
            {
                num3 = PawnRenderer_override.GetAimingRotation(___pawn, localTargetInfo);
            }
        }

        var mainHandAngle = num3;
        var offHandAngle = num3;
        if (yayoCombat.using_dualWeld)
        {
            if (___pawn.equipment.TryGetOffHandEquipment(out var result))
            {
                thingWithComps = result;
            }

            if (thingWithComps != null)
            {
                if (___pawn.GetStancesOffHand() != null)
                {
                    stance_Busy2 = ___pawn.GetStancesOffHand().curStance as Stance_Busy;
                }

                if (localTargetInfo == null && stance_Busy2 is { neverAimWeapon: false })
                {
                    localTargetInfo = stance_Busy2.focusTarg;
                    if (localTargetInfo != null)
                    {
                        num3 = PawnRenderer_override.GetAimingRotation(___pawn, localTargetInfo);
                    }
                }

                mainHandAngle = num3;
                offHandAngle = num3;
                SetAnglesAndOffsets(___pawn.equipment.Primary, thingWithComps, num3, ___pawn, ref offsetMainHand,
                    ref offsetOffHand, ref mainHandAngle, ref offHandAngle,
                    PawnRenderer_override.CurrentlyAiming(stance_Busy),
                    stance_Busy2 != null && PawnRenderer_override.CurrentlyAiming(stance_Busy2));
                if ((stance_Busy2 != null && PawnRenderer_override.CurrentlyAiming(stance_Busy2) ||
                     PawnRenderer_override.CurrentlyAiming(stance_Busy)) && localTargetInfo != null)
                {
                    offHandAngle = PawnRenderer_override.GetAimingRotation(___pawn, localTargetInfo);
                    offsetOffHand.y += 0.1f;
                    rootLoc2 = ___pawn.DrawPos + offsetOffHand;
                }
                else
                {
                    rootLoc2 += offsetOffHand;
                }
            }
        }

        if (___pawn.Rotation == Rot4.West)
        {
            y = -0.4787879f;
            num = -0.05f;
        }

        if (___pawn.Rotation == Rot4.North)
        {
            y = -0.3787879f;
        }

        if (thingWithComps == null || thingWithComps != ___pawn.equipment.Primary)
        {
            PawnRenderer_override.animateEquip(__instance, ___pawn, rootLoc + offsetMainHand, mainHandAngle,
                ___pawn.equipment.Primary, stance_Busy, new Vector3(0f - num, y, 0f - num2));
        }

        if (thingWithComps != null)
        {
            PawnRenderer_override.animateEquip(
                offset: new Vector3(num,
                    ___pawn.Rotation == Rot4.East ? -0.42878792f : !(___pawn.Rotation == Rot4.West) ? 0f : 0.05f, num2),
                __instance: __instance, pawn: ___pawn, rootLoc: rootLoc2, num: offHandAngle, thing: thingWithComps,
                stance_Busy: stance_Busy2, isSub: true);
        }

        return false;
    }

    private static void SetAnglesAndOffsets(Thing eq, ThingWithComps offHandEquip, float aimAngle, Pawn pawn,
        ref Vector3 offsetMainHand, ref Vector3 offsetOffHand, ref float mainHandAngle, ref float offHandAngle,
        bool mainHandAiming, bool offHandAiming)
    {
        if (!yayoCombat.using_dualWeld)
        {
            return;
        }

        var methodInfo = typeof(PawnRenderer_DrawEquipmentAiming).GetMethod("SetAnglesAndOffsets",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var arguments = new object[]
        {
            eq, offHandEquip, aimAngle, pawn, offsetMainHand, offsetOffHand, mainHandAngle,
            offHandAngle, mainHandAiming, offHandAiming
        };
        methodInfo?.Invoke(null, arguments);
        //PawnRenderer_DrawEquipmentAiming.SetAnglesAndOffsets();
    }
}