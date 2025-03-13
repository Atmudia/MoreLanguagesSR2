using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.UI.Localization;
using MelonLoader;

namespace MoreLanguagesMod.Patches;

[HarmonyPatch(typeof(LocalizationDirector))]
public static class Patch_LocalizationDirector
{
    [HarmonyPatch(nameof(Awake)), HarmonyPostfix]
    public static void Awake(LocalizationDirector __instance)
    {
        foreach (var x in LanguageController.AddedLocales.Keys)
        {
            x.CustomFormatterCode = "";
            x.SortOrder = 0;
            __instance.Locales.Add(x);
        }
        MelonCoroutines.Start(EntryPoint.GetAllTables(__instance.Locales.ToArray().FirstOrDefault( x => x.Identifier.Code.Equals("en"))));
    }
}