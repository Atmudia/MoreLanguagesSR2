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
        LanguageController.addedLocales.ForEach(delegate(Locale x)
        {
            x.CustomFormatterCode = "";
            x.SortOrder = 0;
            __instance.Locales.Add(x);
        });
    }
    [HarmonyPatch(nameof(LoadTables)), HarmonyPrefix, HarmonyPriority(800)]
    public static bool LoadTables(LocalizationDirector __instance)
    {      
     string identifierCode = LocalizationSettings.SelectedLocale.Identifier.Code;
      if (LanguageController.addedLocales.FirstOrDefault( x => x.Identifier.Code == identifierCode) == null)
        return true;
      foreach (StringTable cachedStringTable in LanguageController.cachedStringTables)
        UnityEngine.Object.Destroy(cachedStringTable);
      LanguageController.cachedStringTables.Clear();
      DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(MelonEnvironment.MelonBaseDirectory, "MoreLanguages", identifierCode));
      if (!directoryInfo.Exists)
      {
        MelonLogger.Msg("This Language named " + LocalizationSettings.SelectedLocale.name + " doesn't have any folder related to language. LangCode: " + identifierCode);
        return false;
      }
      Il2CppSystem.Collections.Generic.Dictionary<string, StringTable> tables = __instance.Tables;
      if (!tables.ContainsKey("BootUp"))
        tables.Add("BootUp", null);
      if (!tables.ContainsKey("Options"))
        tables.Add("Options", null);
      foreach (Il2CppSystem.Collections.Generic.KeyValuePair<string, StringTable> keyValuePair in tables)
      {
        Il2CppSystem.Collections.Generic.KeyValuePair<string, StringTable> stringKeys = keyValuePair;
        FileInfo fileInfo = new FileInfo(Path.Combine(directoryInfo.FullName, stringKeys.Key + ".json"));
        if (!fileInfo.Exists)
        {
          string message = "This Language named " + LocalizationSettings.SelectedLocale.name + " doesn't have bundle named " + stringKeys.Key + ". LangCode: " + identifierCode;
          MelonLogger.Msg(message);
        }
        else
        {
          StringTable original = EntryPoint.copyTables.FirstOrDefault(x => x.name.Contains(stringKeys.Key));
          StringTable stringTable1 = UnityEngine.Object.Instantiate(original);
          if (original != null)
            stringTable1.name = original.name.Replace("_en(Clone)", "_" + identifierCode);
          stringTable1.LocaleIdentifier = LocalizationSettings.SelectedLocale.Identifier;
          StringTable stringTable2 = stringTable1;
          stringTable2.hideFlags |= HideFlags.HideAndDontSave;
          System.Collections.Generic.Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(File.ReadAllText(fileInfo.FullName));
          foreach (Il2CppSystem.Collections.Generic.KeyValuePair<long, StringTableEntry> mTableEntry in stringTable1.m_TableEntries)
          {
            if (!string.IsNullOrEmpty(mTableEntry.Value.Key) && dictionary.TryGetValue(mTableEntry.Value.Key, out var str))
              mTableEntry.Value.Value = str;
          }
          LanguageController.cachedStringTables.Add(stringTable1);
        }
      }
      foreach (StringTable cachedStringTable in LanguageController.cachedStringTables)
        __instance.Tables[cachedStringTable.SharedData.TableCollectionName] = cachedStringTable;
      __instance.Tables.Remove("BootUp");
      __instance.Tables.Remove("Options");
      return false;
    }
}