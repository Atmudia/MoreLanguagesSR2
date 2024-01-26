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
        if (__instance.font.name.Contains("Runsell Type - HemispheresCaps"))
            LanguageController.InstallHemispheres(__instance);
        if (__instance.font.name.Contains("Lexend-Regular"))
            LanguageController.InstallHumanstSDF(__instance);
    }
}