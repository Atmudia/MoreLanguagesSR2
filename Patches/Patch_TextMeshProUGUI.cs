using System.Globalization;
using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;
using MelonLoader;

namespace MoreLanguagesMod.Patches;

[HarmonyPatch(typeof(TextMeshProUGUI))]
public class Patch_TextMeshProUGUI
{
    [HarmonyPatch(nameof(Awake)), HarmonyPrefix]
    public static void Awake(TextMeshProUGUI __instance)
    {
        if (!__instance.font)
            return;
        if (__instance.font.name.Contains("Runsell Type - HemispheresCaps"))
        {
            LanguageController.InstallHemispheres(__instance);
        }
        else
        {
            LanguageController.InstallLexend(__instance);
        }
    }
}

[HarmonyPatch(typeof(TMP_Text), nameof(TMP_Text.text), MethodType.Setter)]
public static class Patch_TMP_Text
{
    public static CultureInfo Turkish => CultureInfo.GetCultureInfo("tr-TR");
    public static void Prefix(TMP_Text __instance, ref string value)
    {
        var systemContext = SRSingleton<SystemContext>.Instance;
        if (systemContext && systemContext.LocalizationDirector && systemContext.LocalizationDirector.GetCurrentLocaleCode() == "tr")
            if (__instance.font && __instance.font.name.Contains("Runsell Type"))
            {
                value = value.ToUpper(Turkish);
            }
    }
}