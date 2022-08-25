using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace yayoCombat;

[HarmonyPatch(typeof(StatPart_ReloadMarketValue), "TransformAndExplain")]
public class Patch_StatPart_ReloadMarketValue_TransformAndExplain
{
    public static bool Prefix(StatRequest req, ref float val, StringBuilder explanation)
    {
        if (req.Thing is not { def.IsRangedWeapon: true })
        {
            return true;
        }

        var compReloadable = req.Thing.TryGetComp<CompReloadable>();
        if (compReloadable == null)
        {
            return true;
        }

        if (compReloadable.AmmoDef == null || compReloadable.RemainingCharges == 0)
        {
            return false;
        }

        var remainingCharges = compReloadable.RemainingCharges;
        var num = compReloadable.AmmoDef.BaseMarketValue * remainingCharges;
        val += num;
        explanation?.AppendLine(
            "StatsReport_ReloadMarketValue".Translate(compReloadable.AmmoDef.Named("AMMO"),
                remainingCharges.Named("COUNT")) + ": " + num.ToStringMoneyOffset());

        return false;
    }
}