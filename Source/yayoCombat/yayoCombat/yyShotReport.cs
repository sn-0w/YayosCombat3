using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace yayoCombat;

internal class yyShotReport
{
    [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
    public class yayoTryCastShot
    {
        [HarmonyPostfix]
        public static bool Prefix(ref bool __result, Verb_LaunchProjectile __instance, LocalTargetInfo ___currentTarget,
            bool ___canHitNonTargetPawnsNow, bool ___preventFriendlyFire)
        {
            var localTargetInfo = ___currentTarget;
            if (!yayoCombat.advShootAcc || !yayoCombat.turretAcc && !__instance.CasterIsPawn ||
                !yayoCombat.mechAcc && (!__instance.CasterIsPawn || __instance.CasterPawn.RaceProps.IsMechanoid) ||
                yayoCombat.colonistAcc && (!__instance.CasterIsPawn || !__instance.CasterPawn.IsColonist))
            {
                return true;
            }

            if (localTargetInfo.HasThing && localTargetInfo.Thing.Map != __instance.caster.Map)
            {
                __result = false;
                return false;
            }

            var projectile = __instance.Projectile;
            if (projectile == null)
            {
                __result = false;
                return false;
            }

            var resultingLine = new ShootLine();
            if (__instance.verbProps.stopBurstWithoutLos && !__instance.TryFindShootLineFromTo(
                    __instance.caster.Position, localTargetInfo,
                    out resultingLine))
            {
                __result = false;
                return false;
            }

            if (__instance.EquipmentSource != null)
            {
                __instance.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
                __instance.EquipmentSource.GetComp<CompReloadable>()?.UsedOnce();
            }

            var launcher = __instance.caster;
            Thing equipment = __instance.EquipmentSource;
            var compMannable = __instance.caster.TryGetComp<CompMannable>();
            if (compMannable is { ManningPawn: { } })
            {
                launcher = compMannable.ManningPawn;
                equipment = __instance.caster;
            }

            var drawPos = __instance.caster.DrawPos;
            var projectile2 = (Projectile)GenSpawn.Spawn(projectile, resultingLine.Source, __instance.caster.Map);
            if (__instance.verbProps.ForcedMissRadius > 0.5f)
            {
                var num = VerbUtility.CalculateAdjustedForcedMiss(__instance.verbProps.ForcedMissRadius,
                    localTargetInfo.Cell - __instance.caster.Position);
                if (num > 0.5f)
                {
                    var max = GenRadial.NumCellsInRadius(num);
                    var num2 = Rand.Range(0, max);
                    if (num2 > 0)
                    {
                        var intVec = localTargetInfo.Cell + GenRadial.RadialPattern[num2];
                        var projectileHitFlags = ProjectileHitFlags.NonTargetWorld;
                        if (Rand.Chance(yayoCombat.s_missBulletHit))
                        {
                            projectileHitFlags = ProjectileHitFlags.All;
                        }

                        if (!___canHitNonTargetPawnsNow)
                        {
                            projectileHitFlags &= ~ProjectileHitFlags.NonTargetPawns;
                        }

                        projectile2.Launch(launcher, drawPos, intVec, localTargetInfo, projectileHitFlags,
                            ___preventFriendlyFire, equipment);
                        __result = true;
                        return false;
                    }
                }
            }

            var shotReport = ShotReport.HitReportFor(__instance.caster, __instance, localTargetInfo);
            var randomCoverToMissInto = shotReport.GetRandomCoverToMissInto();
            var targetCoverDef = randomCoverToMissInto?.def;
            var num3 = 1f - (shotReport.AimOnTargetChance_IgnoringPosture * 0.5f);
            if (num3 < 0f)
            {
                num3 = 0f;
            }

            var num4 = 0.95f;
            float num5;
            if (__instance.CasterIsPawn)
            {
                num5 = __instance.CasterPawn.skills == null
                    ? yayoCombat.baseSkill / 20f
                    : __instance.CasterPawn.skills.GetSkill(SkillDefOf.Shooting).levelInt / 20f;
                num4 = 1f - (__instance.caster.GetStatValue(StatDefOf.ShootingAccuracyPawn) * num5);
            }
            else
            {
                num5 = yayoCombat.baseSkill / 20f;
                num4 = 1f - (num4 * num5);
            }

            var lengthHorizontal = (localTargetInfo.Cell - __instance.caster.Position).LengthHorizontal;
            _ = 1f - __instance.verbProps.GetHitChanceFactor(__instance.EquipmentSource, lengthHorizontal);
            var num7 = 1f;
            var num8 = __instance.caster.Position.Roofed(__instance.caster.Map) &&
                       localTargetInfo.Cell.Roofed(__instance.caster.Map)
                ? 1f
                : __instance.caster.Map.weatherManager.CurWeatherAccuracyMultiplier;
            var num9 = num7 * num8;
            if (__instance.EquipmentSource != null &&
                __instance.EquipmentSource.def.equipmentType != EquipmentType.None)
            {
                num3 *= (((yayoCombat.s_accEf * num4) + (1f - yayoCombat.s_accEf)) * num9) + (1f - num9);
            }

            if (lengthHorizontal < 10f)
            {
                num3 -= Mathf.Clamp((10f - lengthHorizontal) * 0.07f, 0f, 0.3f);
            }

            num3 = (num3 * 0.95f) + 0.05f;
            Mathf.Clamp(num3, 0.05f, 0.95f);
            if (Rand.Chance(num3))
            {
                resultingLine.ChangeDestToMissWild(shotReport.AimOnTargetChance_StandardTarget);
                var projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
                if (Rand.Chance(yayoCombat.s_missBulletHit) && ___canHitNonTargetPawnsNow)
                {
                    projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
                }

                projectile2.Launch(launcher, drawPos, resultingLine.Dest, localTargetInfo, projectileHitFlags2,
                    ___preventFriendlyFire, equipment, targetCoverDef);
                __result = true;
                return false;
            }

            if (localTargetInfo.Thing != null && localTargetInfo.Thing.def.category == ThingCategory.Pawn &&
                !Rand.Chance(shotReport.PassCoverChance))
            {
                var projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld;
                if (___canHitNonTargetPawnsNow)
                {
                    projectileHitFlags3 |= ProjectileHitFlags.NonTargetPawns;
                }

                projectile2.Launch(launcher, drawPos, randomCoverToMissInto, localTargetInfo, projectileHitFlags3,
                    ___preventFriendlyFire, equipment, targetCoverDef);
                __result = true;
                return false;
            }

            var projectileHitFlags4 = ProjectileHitFlags.IntendedTarget;
            if (___canHitNonTargetPawnsNow)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetPawns;
            }

            if (!localTargetInfo.HasThing || localTargetInfo.Thing.def.Fillage == FillCategory.Full)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
            }

            projectile2.Launch(launcher, drawPos, localTargetInfo.Thing != null ? localTargetInfo : resultingLine.Dest,
                localTargetInfo, projectileHitFlags4,
                ___preventFriendlyFire, equipment, targetCoverDef);

            __result = true;
            return false;
        }
    }
}