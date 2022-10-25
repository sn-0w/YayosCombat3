using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace yayoCombat;

public static class PawnRenderer_override
{
    public static void DrawEquipmentAiming(this PawnRenderer instance, Thing eq, Vector3 drawLoc, float aimAngle,
        Pawn pawn, Stance_Busy stance_Busy = null, bool pffhand = false)
    {
        if (!yayoCombat.advAni)
        {
            instance.DrawEquipmentAiming(eq, drawLoc, aimAngle);
            return;
        }

        var num = aimAngle - 90f;

        Mesh mesh;
        switch (aimAngle)
        {
            case > 20f and < 160f:
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
                break;
            case > 200f and < 340f:
                mesh = MeshPool.plane10Flip;
                num -= 180f;
                num -= eq.def.equippedAngleOffset;
                break;
            default:
                mesh = MeshPool.plane10;
                num += eq.def.equippedAngleOffset;
                break;
        }

        num %= 360f;

        if (yayoCombat.using_Oversized)
        {
            SaveWeaponLocation(eq, GetDrawOffset(drawLoc, eq, pawn), aimAngle);
            drawLoc += yayoCombat.GetOversizedOffset(pawn, eq as ThingWithComps);
        }

        var position = GetDrawOffset(drawLoc, eq, pawn);

        Graphics.DrawMesh(
            material: !(eq.Graphic is Graphic_StackCount graphic_StackCount)
                ? eq.Graphic.MatSingle
                : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle,
            mesh: GetMesh(mesh, eq, aimAngle, pawn), position: position,
            rotation: Quaternion.AngleAxis(num, Vector3.up), layer: 0);
        if (yayoCombat.using_Oversized)
        {
            return;
        }

        SaveWeaponLocation(eq, position, aimAngle);
    }


    public static void SaveWeaponLocation(Thing weapon, Vector3 drawLoc, float aimAngle)
    {
        if (!yayoCombat.using_showHands)
        {
            return;
        }

        if (yayoCombat.weaponLocations == null)
        {
            var weaponLocationsField = AccessTools.TypeByName("ShowMeYourHandsMain").GetField("weaponLocations");
            yayoCombat.weaponLocations = (Dictionary<Thing, Tuple<Vector3, float>>)weaponLocationsField?.GetValue(null);
        }

        if (yayoCombat.weaponLocations == null)
        {
            return;
        }

        yayoCombat.weaponLocations[weapon] = new Tuple<Vector3, float>(drawLoc, aimAngle);
    }

    public static void animateEquip(PawnRenderer __instance, Pawn pawn, Vector3 rootLoc, float num,
        ThingWithComps thing, Stance_Busy stance_Busy, Vector3 offset, bool isSub = false)
    {
        var vector = rootLoc;
        var isMechanoid = pawn.RaceProps.IsMechanoid;
        if (stance_Busy is { neverAimWeapon: false, focusTarg.IsValid: true })
        {
            if (thing.def.IsRangedWeapon && !stance_Busy.verb.IsMeleeAttack)
            {                
                var num2 = stance_Busy.verb.ticksToNextBurstShot;
                var num3 = (pawn.LastAttackTargetTick + thing.thingIDNumber) % 10000 % 1000 % 100 % 5;
                var stance_Warmup = pawn.stances.curStance as Stance_Warmup;
                if (num2 > 10)
                {
                    num2 = 10;
                }

                float num4 = num2;
                float num5 = stance_Busy.ticksLeft;
                var num6 = 0f;
                if (!isMechanoid)
                {
                    num6 = Mathf.Max(num5, 25f) * 0.001f;
                }

                if (num2 > 0)
                {
                    num6 = num4 * 0.02f;
                }

                var num7 = 0f;
                var num8 = offset.x;
                var num9 = offset.z;
                if (!isMechanoid)
                {
                    var num10 = isSub ? Mathf.Sin((num5 * 0.035f) + 0.5f) * 0.05f : Mathf.Sin(num5 * 0.035f) * 0.05f;
                    switch (num3)
                    {
                        case 0:
                            if (!(yayoCombat.ani_twirl && !pawn.RaceProps.IsMechanoid && thing.def.BaseMass < 5f) &&
                                stance_Busy.ticksLeft > 1)
                            {
                                num9 += num10;
                            }

                            break;
                        case 1:
                            if (num2 == 0 && stance_Busy.ticksLeft <= 78)
                            {
                                if (stance_Busy.ticksLeft > 48 && stance_Warmup == null)
                                {
                                    var num11 = Mathf.Sin(num5 * 0.1f) * 0.05f;
                                    num8 += num11 - 0.2f;
                                    num9 += num11 + 0.2f;
                                    num7 += num11 + 30f + (num5 * 0.5f);
                                }
                                else if (stance_Busy.ticksLeft > 40 && stance_Warmup == null)
                                {
                                    var num12 = Mathf.Sin(num5 * 0.1f) * 0.05f;
                                    var num13 = Mathf.Sin(num5) * 0.05f;
                                    num8 += num13 + 0.05f;
                                    num9 += num12 - 0.05f;
                                    num7 += (num13 * 100f) - 15f;
                                }
                                else if (stance_Busy.ticksLeft > 1)
                                {
                                    num9 += num10;
                                }
                            }

                            break;
                        default:
                            if (stance_Busy.ticksLeft > 1)
                            {
                                num9 += num10;
                            }

                            break;
                    }
                }

                var zero = !(pawn.Rotation == Rot4.West)
                    ? vector + new Vector3(0f - num9, offset.y, 0.4f + num8 - num6).RotatedBy(num)
                    : vector + new Vector3(num9, offset.y, 0.4f + num8 - num6).RotatedBy(num);
                if (offset.y >= 0f)
                {
                    zero.y += 5f / 132f;
                }
                else
                {
                    zero.y += 5f / 132f;
                }

                var num14 = 70f;
                if (pawn.Rotation == Rot4.South)
                {
                    __instance.DrawEquipmentAiming(thing, zero, num - (num6 * num14) - num7, pawn);
                }

                if (pawn.Rotation == Rot4.North)
                {
                    __instance.DrawEquipmentAiming(thing, zero, num - (num6 * num14) - num7, pawn);
                }

                if (pawn.Rotation == Rot4.East)
                {
                    __instance.DrawEquipmentAiming(thing, zero, num - (num6 * num14) - num7, pawn);
                }

                if (pawn.Rotation == Rot4.West)
                {
                    __instance.DrawEquipmentAiming(thing, zero, num - (num6 * num14) - num7, pawn);
                }

                return;
            }

            var num16 = (pawn.LastAttackTargetTick + thing.thingIDNumber) % 10000 % 1000 % 100 % 3;
            var num15 = num16 switch
            {
                1 => 25f,
                2 => -25f,
                _ => 0f
            };
            if (thing.def.IsRangedWeapon)
            {
                num15 -= 35f;
            }

            var num17 = isSub && pawn.Rotation == Rot4.West ? -0.2f : 0.2f;
            var num18 = GetAimingRotation(pawn, stance_Busy.focusTarg);
            if (stance_Busy.ticksLeft > 0)
            {
                var num19 = Mathf.Min(stance_Busy.ticksLeft, 60f);
                var num20 = num19 * 0.0075f;
                var x = offset.x;
                var z = offset.z;
                switch (num16)
                {
                    default:
                        x += num17 + 0.05f + num20;
                        z += -0.050000012f - (num20 * 0.1f);
                        break;
                    case 1:
                        x += num17 + 0.05f + num20;
                        z += 0.099999994f + (num20 * 0.5f);
                        num19 = 30f + (num19 * 0.5f);
                        break;
                    case 2:
                        x += num17 + 0.05f + num20;
                        z += 0.099999994f - num20;
                        break;
                }

                if (!(yayoCombat.ani_twirl && !pawn.RaceProps.IsMechanoid && thing.def.BaseMass < 5f) ||
                    pawn.LastAttackTargetTick % 5 != 0 || stance_Busy.ticksLeft <= 25)
                {
                }

                if (pawn.Rotation == Rot4.South)
                {
                    var drawLoc = vector + new Vector3(0f - z, offset.y, x).RotatedBy(num18);
                    if (offset.y >= 0f)
                    {
                        drawLoc.y += 5f / 132f;
                    }

                    num18 += num15;
                    __instance.DrawEquipmentAiming(thing, drawLoc, num + num19, pawn);
                }

                if (pawn.Rotation == Rot4.North)
                {
                    var drawLoc2 = vector + new Vector3(0f - z, offset.y, x).RotatedBy(num18);
                    if (offset.y >= 0f)
                    {
                        drawLoc2.y += 5f / 132f;
                    }

                    num18 += num15;
                    __instance.DrawEquipmentAiming(thing, drawLoc2, num + num19, pawn);
                }

                if (pawn.Rotation == Rot4.East)
                {
                    var drawLoc3 = vector + new Vector3(z, offset.y, x).RotatedBy(num18);
                    if (offset.y >= 0f)
                    {
                        drawLoc3.y += 5f / 132f;
                    }

                    num18 += num15;
                    __instance.DrawEquipmentAiming(thing, drawLoc3, num18 + num19, pawn);
                }

                if (pawn.Rotation != Rot4.West)
                {
                    return;
                }

                var drawLoc4 = vector + new Vector3(z, offset.y, x).RotatedBy(num18);
                if (offset.y >= 0f)
                {
                    drawLoc4.y += 5f / 132f;
                }

                num18 -= num15;
                __instance.DrawEquipmentAiming(thing, drawLoc4, num18 + num19, pawn);
            }
            else
            {
                var drawLoc5 = vector + new Vector3(isSub && pawn.Rotation == Rot4.West ? -0.2f : 0f, offset.y, num17)
                    .RotatedBy(num);
                if (offset.y >= 0f)
                {
                    drawLoc5.y += 5f / 132f;
                }

                __instance.DrawEquipmentAiming(thing, drawLoc5, num, pawn);
            }
        }
        else
        {
            if (pawn.carryTracker is { CarriedThing: { } } || !pawn.Drafted &&
                (pawn.CurJob == null || !pawn.CurJob.def.alwaysShowWeapon) &&
                (pawn.mindState.duty == null || !pawn.mindState.duty.def.alwaysShowWeapon))
            {
                return;
            }

            var num21 = Mathf.Abs(pawn.HashOffsetTicks() % 1000000000);
            num21 %= 100000000;
            num21 %= 10000000;
            num21 %= 1000000;
            num21 %= 100000;
            num21 %= 10000;
            num21 %= 1000;
            var num22 = isSub ? Mathf.Sin((num21 * 0.05f) + 0.5f) : Mathf.Sin(num21 * 0.05f);
            var num23 = -5f;
            var num24 = 0f;
            if (yayoCombat.ani_twirl && !pawn.RaceProps.IsMechanoid && thing.def.BaseMass < 5f)
            {
                if (!isSub)
                {
                    if (num21 is < 80 and >= 40)
                    {
                        num24 += num21 * 36f;
                        vector += new Vector3(-0.2f, 0f, 0.1f);
                    }
                }
                else if (num21 < 40)
                {
                    num24 += (num21 - 40) * -36f;
                    vector += new Vector3(0.2f, 0f, 0.1f);
                }
            }

            if (pawn.Rotation == Rot4.South)
            {
                Vector3 zero2;
                var num25 = num;
                if (!isSub)
                {
                    zero2 = vector + new Vector3(0f, offset.y, -0.22f + (num22 * 0.05f));
                }
                else
                {
                    zero2 = vector + new Vector3(0f, offset.y, -0.22f + (num22 * 0.05f));
                    num25 = 350f - num;
                    num23 *= -1f;
                }

                if (offset.y >= 0f)
                {
                    zero2.y += 5f / 132f;
                }

                __instance.DrawEquipmentAiming(thing, zero2, num24 + num25 + (num22 * num23), pawn);
            }
            else if (pawn.Rotation == Rot4.North)
            {
                Vector3 zero3;
                var num26 = num;
                if (!isSub)
                {
                    zero3 = vector + new Vector3(0f, offset.y, -0.11f + (num22 * 0.05f));
                }
                else
                {
                    zero3 = vector + new Vector3(0f, offset.y, -0.11f + (num22 * 0.05f));
                    num26 = 350f - num;
                    num23 *= -1f;
                }

                zero3.y += 0f;
                __instance.DrawEquipmentAiming(thing, zero3, num24 + num26 + (num22 * num23), pawn);
            }
            else if (pawn.Rotation == Rot4.East)
            {
                Vector3 zero4;
                if (!isSub)
                {
                    zero4 = vector + new Vector3(0.2f, offset.y, -0.22f + (num22 * 0.05f));
                }
                else
                {
                    zero4 = vector + new Vector3(0.2f, offset.y, -0.22f + (num22 * 0.05f));
                    num23 *= -1f;
                }

                if (offset.y >= 0f)
                {
                    zero4.y += 5f / 132f;
                }

                __instance.DrawEquipmentAiming(thing, zero4, num24 + num + (num22 * num23), pawn);
            }
            else if (pawn.Rotation == Rot4.West)
            {
                Vector3 zero5;
                var num27 = 350f - num;
                if (!isSub)
                {
                    zero5 = vector + new Vector3(-0.2f, offset.y, -0.22f + (num22 * 0.05f));
                }
                else
                {
                    zero5 = vector + new Vector3(-0.2f, offset.y, -0.22f + (num22 * 0.05f));
                    num23 *= -1f;
                }

                if (offset.y >= 0f)
                {
                    zero5.y += 5f / 132f;
                }

                __instance.DrawEquipmentAiming(thing, zero5, num24 + num27 + (num22 * num23), pawn);
            }
        }
    }

    internal static float GetAimingRotation(Pawn pawn, LocalTargetInfo focusTarg)
    {
        var vector = !focusTarg.HasThing ? focusTarg.Cell.ToVector3Shifted() : focusTarg.Thing.DrawPos;
        var result = 0f;
        if ((vector - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
        {
            result = (vector - pawn.DrawPos).AngleFlat();
        }

        return result;
    }

    internal static bool CurrentlyAiming(Stance_Busy stance)
    {
        return stance is { neverAimWeapon: false, focusTarg.IsValid: true };
    }

    internal static bool IsMeleeWeapon(ThingWithComps eq)
    {
        var compEquippable = eq?.TryGetComp<CompEquippable>();
        return compEquippable != null && compEquippable.PrimaryVerb.IsMeleeAttack;
    }

    public static Vector3 GetDrawOffset(Vector3 drawLoc, Thing thing, Pawn pawn)
    {
        if (thing.def.apparel?.wornGraphicData == null)
        {
            return drawLoc + thing.def.graphicData.DrawOffsetForRot(pawn.Rotation);
        }

        var zero = thing.def.apparel.wornGraphicData.OffsetAt(pawn.Rotation, pawn.story.bodyType);
        zero.y = thing.def.graphicData.DrawOffsetForRot(pawn.Rotation).y;
        return drawLoc + zero;
    }

    public static Mesh GetMesh(Mesh mesh, Thing thing, float aimAngle, Pawn pawn)
    {
        Vector2 size;
        if (pawn.RaceProps.Humanlike)
        {
            if (yayoCombat.using_AlienRaces)
            {
                var vector = AlienRaceUtility.AlienRacesPatch(pawn, thing);
                var num = Mathf.Max(vector.x, vector.y);
                size = new Vector2(thing.def.graphicData.drawSize.x * num, thing.def.graphicData.drawSize.y * num);
            }
            else
            {
                size = new Vector2(thing.def.graphicData.drawSize.x, thing.def.graphicData.drawSize.y);
            }
        }
        else
        {
            var drawSize = pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize;
            size = new Vector2(thing.def.graphicData.drawSize.x + (drawSize.x / 10f),
                thing.def.graphicData.drawSize.y + (drawSize.y / 10f));
        }

        if (thing.def.graphicData.drawSize == Vector2.one)
        {
            return mesh;
        }

        mesh = !(aimAngle > 200f) || !(aimAngle < 340f) ? MeshPool.GridPlane(size) : MeshPool.GridPlaneFlip(size);
        return mesh;
    }

    public static Vector3 OffsetAt(this WornGraphicData graphicData, Rot4 facing, BodyTypeDef bodyType = null)
    {
        var wornGraphicDirectionData = default(WornGraphicDirectionData);
        switch (facing.AsInt)
        {
            case 0:
                wornGraphicDirectionData = graphicData.north;
                break;
            case 1:
                wornGraphicDirectionData = graphicData.east;
                break;
            case 2:
                wornGraphicDirectionData = graphicData.south;
                break;
            case 3:
                wornGraphicDirectionData = graphicData.west;
                break;
        }

        var offset = wornGraphicDirectionData.offset;
        if (bodyType == null)
        {
            return new Vector3(offset.x, 0f, offset.y);
        }

        if (bodyType == BodyTypeDefOf.Male)
        {
            offset += wornGraphicDirectionData.male.offset;
        }
        else if (bodyType == BodyTypeDefOf.Female)
        {
            offset += wornGraphicDirectionData.female.offset;
        }
        else if (bodyType == BodyTypeDefOf.Thin)
        {
            offset += wornGraphicDirectionData.thin.offset;
        }
        else if (bodyType == BodyTypeDefOf.Hulk)
        {
            offset += wornGraphicDirectionData.hulk.offset;
        }
        else if (bodyType == BodyTypeDefOf.Fat)
        {
            offset += wornGraphicDirectionData.fat.offset;
        }

        return new Vector3(offset.x, 0f, offset.y);
    }
}