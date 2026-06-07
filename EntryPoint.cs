using System.Collections;
using HarmonyLib;
using MelonLoader;
using MoreLanguagesMod;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Locale = UnityEngine.Localization.Locale;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(EntryPoint), "MoreLanguagesMod", "1.4", "Atmudia", "https://www.nexusmods.com/slimerancher2/mods/31")]
[assembly: MelonGame("MonomiPark", "SlimeRancher2")]
namespace MoreLanguagesMod
{
  [HarmonyPatch]
  internal class EntryPoint : MelonMod
  {
    public static List<StringTable> copyTables = new System.Collections.Generic.List<StringTable>();
    public static List<AssetTable> copyAssetTables = new System.Collections.Generic.List<AssetTable>();

    public static IEnumerator GetAllTables(Locale locale)
    {
      AsyncOperationHandle<Il2CppSystem.Collections.Generic.IList<StringTable>> stringTables = LocalizationSettings.StringDatabase.GetAllTables(locale);
      yield return stringTables;
      Il2CppSystem.Collections.Generic.List<StringTable> stringTableList = new Il2CppSystem.Collections.Generic.List<StringTable>(stringTables.Result.Cast<Il2CppSystem.Collections.Generic.IEnumerable<StringTable>>());
      foreach (StringTable stringTable1 in stringTableList)
      {
        if (RegenerateTranslationsUtils.ifRegenerate)
          RegenerateTranslationsUtils.AddItToList(stringTable1);
        StringTable instantiate = Object.Instantiate(stringTable1);
        instantiate.hideFlags |= HideFlags.HideAndDontSave;
        instantiate.SharedData = Object.Instantiate(instantiate.SharedData);
        copyTables.Add(instantiate);
      }
      AsyncOperationHandle<Il2CppSystem.Collections.Generic.IList<AssetTable>> assetTables =
        LocalizationSettings.AssetDatabase.GetAllTables(locale);
      
     yield return assetTables;
     Il2CppSystem.Collections.Generic.List<AssetTable> assetTableList = new Il2CppSystem.Collections.Generic.List<AssetTable>(assetTables.Result.Cast<Il2CppSystem.Collections.Generic.IEnumerable<AssetTable>>());
     
     foreach (var assetTable in assetTableList)
     {
      AssetTable instantiate = Object.Instantiate(assetTable);
      instantiate.hideFlags |= HideFlags.HideAndDontSave;
      instantiate.SharedData = Object.Instantiate(instantiate.SharedData);
      copyAssetTables.Add(instantiate);
     }
     
      if (RegenerateTranslationsUtils.ifRegenerate)
        RegenerateTranslationsUtils.ConvertDirectoryIntoFiles();
      try
      {
        foreach (var localeAdded in LanguageController.AddedLocales)
        {
          foreach (var cloned in copyTables.Select(original => LanguageController.CreateClonedStringTable(original, localeAdded.Key, localeAdded.Value)))
          {
            LanguageController.CachedStringTables.Add(cloned.name, cloned);
          }
          foreach (var assetTable in copyAssetTables.Select(x => MoreLanguagesMod.LanguageController.CreateClonedAssetTable(x, localeAdded.Key)))
          {
            LanguageController.CachedAssetTables.Add(assetTable.name, assetTable);
          }
          
          
          LanguageController.AddResourceLocationsForLocale(localeAdded.Key);

         
        }
      }
      catch (Exception e)
      {
        MelonLogger.Error(e);
      }
      
      
    }
    [HarmonyPatch(typeof(ResourceManager), nameof(ResourceManager.ProvideResource), typeof(IResourceLocation), typeof(Il2CppSystem.Type), typeof(bool))]
    [HarmonyPrefix]
    public static bool Patch_ProvideResource(ResourceManager __instance, IResourceLocation location, ref AsyncOperationHandle __result)
    {
      if (location == null || !location.PrimaryKey.Contains("MoreLanguagesMod/")) return true;
      
      if (LanguageController.CachedStringTables.TryGetValue(location.PrimaryKey.Replace("MoreLanguagesMod/", string.Empty), out var stringTable))
      {
        __result = __instance.CreateCompletedOperation(stringTable, string.Empty);
        return false;
      }

      if (LanguageController.CachedAssetTables.TryGetValue(location.PrimaryKey.Replace("MoreLanguagesMod/", string.Empty), out var assetTable))
      {
        __result = __instance.CreateCompletedOperation(assetTable, string.Empty);
        return false;

      }
      return true;        
    }
    [HarmonyPatch(typeof(Addressables), nameof(Addressables.LoadResourceLocationsAsync), typeof(Il2CppSystem.Object), typeof(Il2CppSystem.Type))]
    [HarmonyPrefix]
    public static bool LoadResourceLocationsAsync(Il2CppSystem.Object key, Il2CppSystem.Type type, ref AsyncOperationHandle<Il2CppSystem.Collections.Generic.IList<IResourceLocation>> __result)
    {
      if (key == null)
        return true;
      foreach (var locale in LanguageController.AddedLocales.Keys)
      {
        if (key.ToString().Contains($"_{locale.Identifier.Code}") && LanguageController.ResourceLocationBases.TryGetValue("MoreLanguagesMod/" + key.ToString(), out var value))
        {
          __result = Addressables.ResourceManager.CreateCompletedOperation(value, string.Empty);
          return false;
        }      
      }
      return true;

    }
    
    public override void OnInitializeMelon()
    {
      LanguageController.Setup();
      LanguageController.InstallLocale(Locale.CreateLocale(SystemLanguage.Polish), typeof(EntryPoint).Assembly);
      LanguageController.InstallLocale(Locale.CreateLocale(SystemLanguage.Czech), typeof(EntryPoint).Assembly);
      LanguageController.InstallLocale(Locale.CreateLocale(SystemLanguage.Turkish), typeof(EntryPoint).Assembly);

    }
  }

}
