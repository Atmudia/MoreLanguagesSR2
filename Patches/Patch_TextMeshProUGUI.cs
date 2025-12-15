using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;

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
    private static string ToTurkishUpperInvariant(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return input
            .Replace("i", "İ") // dotted i
            .Replace("ı", "I") // dotless i
            .ToUpperInvariant();
    }
    public static void Prefix(TMP_Text __instance, ref string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        var systemContext = SRSingleton<SystemContext>.Instance;
        if (!systemContext || systemContext.LocalizationDirector == null)
            return;

        if (systemContext.LocalizationDirector.GetCurrentLocaleCode() != "tr")
            return;

        if (__instance.font && __instance.font.name.Contains("Runsell Type"))
        {
            value = ToTurkishUpperInvariant(value);
        }
    }

}