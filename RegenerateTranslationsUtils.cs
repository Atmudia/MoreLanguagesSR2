using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace MoreLanguagesMod
{
  internal static class RegenerateTranslationsUtils
  {
    internal static bool ifRegenerate = false;
    private static System.Collections.Generic.List<string> catchedLists = new System.Collections.Generic.List<string>();
    private static System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>> translations = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>();

    internal static void AddItToList(StringTable table)
    {
      if (catchedLists.Contains(table.name))
        return;
      catchedLists.Add(table.name);
      foreach (Il2CppSystem.Collections.Generic.KeyValuePair<long, StringTableEntry> mTableEntry in table.m_TableEntries)
      {
        if (!string.IsNullOrEmpty(mTableEntry.Value.Key))
        {
          string key = table.SharedData.name.Replace(" Shared Data", string.Empty);
          if (!translations.ContainsKey(key))
            translations.Add(key, new System.Collections.Generic.Dictionary<string, string>());
          translations[key].Add(mTableEntry.Value.Key ?? "", mTableEntry.Value.Value);
        }
      }
    }

    internal static void ConvertDirectoryIntoFiles()
    {
      foreach (System.Collections.Generic.KeyValuePair<string, System.Collections.Generic.Dictionary<string, string>> translation in translations)
        File.WriteAllText("D:\\SteamLibrary\\steamapps\\common\\Slime Rancher 2\\MoreLanguages\\enNew\\" + translation.Key + ".json", JsonConvert.SerializeObject(translations[translation.Key], (Formatting) 1));
    }
  }
}
