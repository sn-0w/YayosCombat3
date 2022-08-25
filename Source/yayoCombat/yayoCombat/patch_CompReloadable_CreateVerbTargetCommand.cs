using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(CompReloadable), "CreateVerbTargetCommand")]
internal class patch_CompReloadable_CreateVerbTargetCommand
{
    [HarmonyPostfix]
    private static bool Prefix(ref Command_Reloadable __result, CompReloadable __instance, Thing gear, Verb verb)
    {
        if (!yayoCombat.ammo)
        {
            return true;
        }

        if (gear.def.IsWeapon)
        {
            var command_Reloadable = new Command_Reloadable(__instance)
            {
                defaultDesc = gear.def.description,
                defaultLabel = verb.verbProps.label,
                verb = verb
            };
            if (verb.verbProps.defaultProjectile != null && verb.verbProps.commandIcon == null)
            {
                command_Reloadable.icon = verb.verbProps.defaultProjectile.uiIcon;
                command_Reloadable.iconAngle = verb.verbProps.defaultProjectile.uiIconAngle;
                command_Reloadable.iconOffset = verb.verbProps.defaultProjectile.uiIconOffset;
                if (verb.verbProps.defaultProjectile.graphicData != null)
                {
                    command_Reloadable.overrideColor = verb.verbProps.defaultProjectile.graphicData.color;
                }
            }
            else
            {
                command_Reloadable.icon = verb.UIIcon != BaseContent.BadTex ? verb.UIIcon : gear.def.uiIcon;
                command_Reloadable.iconAngle = gear.def.uiIconAngle;
                command_Reloadable.iconOffset = gear.def.uiIconOffset;
                command_Reloadable.defaultIconColor = gear.DrawColor;
            }

            if (!__instance.Wearer.IsColonistPlayerControlled || !__instance.Wearer.Drafted)
            {
                command_Reloadable.Disable();
            }
            else if (verb.verbProps.violent && __instance.Wearer.WorkTagIsDisabled(WorkTags.Violent))
            {
                command_Reloadable.Disable("IsIncapableOfViolenceLower"
                    .Translate(__instance.Wearer.LabelShort, __instance.Wearer).CapitalizeFirst() + ".");
            }
            else if (!__instance.CanBeUsed)
            {
                command_Reloadable.Disable(__instance.DisabledReason(__instance.MinAmmoNeeded(false),
                    __instance.MaxAmmoNeeded(false)));
            }

            __result = command_Reloadable;
            return false;
        }

        var command_Reloadable2 = new Command_Reloadable(__instance)
        {
            defaultDesc = gear.def.description,
            hotKey = __instance.Props.hotKey,
            defaultLabel = verb.verbProps.label,
            verb = verb
        };
        if (verb.verbProps.defaultProjectile != null && verb.verbProps.commandIcon == null)
        {
            command_Reloadable2.icon = verb.verbProps.defaultProjectile.uiIcon;
            command_Reloadable2.iconAngle = verb.verbProps.defaultProjectile.uiIconAngle;
            command_Reloadable2.iconOffset = verb.verbProps.defaultProjectile.uiIconOffset;
            command_Reloadable2.overrideColor = verb.verbProps.defaultProjectile.graphicData.color;
        }
        else
        {
            command_Reloadable2.icon = verb.UIIcon != BaseContent.BadTex ? verb.UIIcon : gear.def.uiIcon;
            command_Reloadable2.iconAngle = gear.def.uiIconAngle;
            command_Reloadable2.iconOffset = gear.def.uiIconOffset;
            command_Reloadable2.defaultIconColor = gear.DrawColor;
        }

        if (!__instance.Wearer.IsColonistPlayerControlled)
        {
            command_Reloadable2.Disable();
        }
        else if (verb.verbProps.violent && __instance.Wearer.WorkTagIsDisabled(WorkTags.Violent))
        {
            command_Reloadable2.Disable("IsIncapableOfViolenceLower"
                .Translate(__instance.Wearer.LabelShort, __instance.Wearer).CapitalizeFirst() + ".");
        }
        else if (!__instance.CanBeUsed)
        {
            command_Reloadable2.Disable(__instance.DisabledReason(__instance.MinAmmoNeeded(false),
                __instance.MaxAmmoNeeded(false)));
        }

        __result = command_Reloadable2;
        return false;
    }
}