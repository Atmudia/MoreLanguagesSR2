using HarmonyLib;
using Il2CppTMPro;
using MelonLoader;

namespace MoreLanguagesMod.Patches;

[HarmonyPatch(typeof(TextMeshProUGUI))]
public class Patch_TextMeshProUGUI
{
    [HarmonyPatch(nameof(Awake)), HarmonyPrefix]
    public static void Awake(TextMeshProUGUI __instance)
    {
        if (__instance.font == null)
            return;
        if (__instance.font.name.Contains("Runsell Type - HemispheresCaps"))
            LanguageController.InstallHemispheres(__instance);
        if (__instance.font.name.Contains("Lexend-Regular") || __instance.font.name.Contains("Lexend-Medium"))
            LanguageController.InstallHumanstSDF(__instance);
    }
}