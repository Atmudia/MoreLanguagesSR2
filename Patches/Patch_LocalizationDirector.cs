using System.IO;
using System.Linq;
using HarmonyLib;
using Il2CppMonomiPark.SlimeRancher.UI.Localization;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace MoreLanguagesMod.Patches;

[HarmonyPatch(typeof(LocalizationDirector))]
public static class Patch_LocalizationDirector
{
    [HarmonyPatch(nameof(Awake)), HarmonyPostfix]
    public static void Awake(LocalizationDirector __instance)
    {
        if (!EntryPoint.Activated)
            return;
        LanguageController.AddedLocales.ForEach(delegate(Locale x)
        {
            x.CustomFormatterCode = "";
            x.SortOrder = 0;
            __instance.Locales.Add(x);
        });
        MelonCoroutines.Start(EntryPoint.GetAllTables(__instance.Locales.ToArray().FirstOrDefault( x => x.Identifier.Code.Equals("en"))));
    }
}