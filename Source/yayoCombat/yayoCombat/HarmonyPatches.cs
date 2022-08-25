using System.Reflection;
using HarmonyLib;
using Verse;

namespace yayoCombat;

public class HarmonyPatches : Mod
{
    public HarmonyPatches(ModContentPack content)
        : base(content)
    {
        var harmony = new Harmony("com.yayo.combat");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}