using DualWield;
using RimWorld;
using Verse;

namespace yayoCombat;

internal static class DualWieldUtility
{
    public static bool TryGetOffHandEquipment(this Pawn_EquipmentTracker instance, out ThingWithComps result)
    {
        result = null;
        if (instance.pawn.HasMissingArmOrHand())
        {
            return false;
        }

        var extendedDataStorage = Base.Instance.GetExtendedDataStorage();
        foreach (var item in instance.AllEquipmentListForReading)
        {
            if (!extendedDataStorage.TryGetExtendedDataFor(item, out var val) || !val.isOffHand)
            {
                continue;
            }

            result = item;
            return true;
        }

        return false;
    }

    public static bool HasMissingArmOrHand(this Pawn instance)
    {
        var result = false;
        foreach (var missingPartsCommonAncestor in instance.health.hediffSet.GetMissingPartsCommonAncestors())
        {
            if (missingPartsCommonAncestor.Part.def == BodyPartDefOf.Hand ||
                missingPartsCommonAncestor.Part.def == BodyPartDefOf.Arm)
            {
                result = true;
            }
        }

        return result;
    }

    public static Pawn_StanceTracker GetStancesOffHand(this Pawn instance)
    {
        var extendedDataStorage = Base.Instance.GetExtendedDataStorage();
        return extendedDataStorage?.GetExtendedDataFor(instance).stancesOffhand;
    }
}