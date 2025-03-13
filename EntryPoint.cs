using System.Collections;
using HarmonyLib;
using Il2CppInterop.Runtime;
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

[assembly: MelonInfo(typeof(EntryPoint), "MoreLanguagesMod", "1.2", "Atmudia", "https://www.nexusmods.com/slimerancher2/mods/31")]
namespace MoreLanguagesMod
{
  [HarmonyPatch]
  internal class EntryPoint : MelonMod
  {
    public static List<StringTable> copyTables = new System.Collections.Generic.List<StringTable>();

    public static IEnumerator GetAllTables(Locale locale)
    {
      AsyncOperationHandle<Il2CppSystem.Collections.Generic.IList<StringTable>> asyncOperationHandle = LocalizationSettings.StringDatabase.GetAllTables(locale);
      yield return asyncOperationHandle;
      Il2CppSystem.Collections.Generic.List<StringTable> list = new Il2CppSystem.Collections.Generic.List<StringTable>(asyncOperationHandle.Result.Cast<Il2CppSystem.Collections.Generic.IEnumerable<StringTable>>());
      foreach (StringTable stringTable1 in list)
      {
        if (RegenerateTranslationsUtils.ifRegenerate)
          RegenerateTranslationsUtils.AddItToList(stringTable1);
        StringTable instantiate = Object.Instantiate(stringTable1);
        instantiate.hideFlags |= HideFlags.HideAndDontSave;
        instantiate.SharedData = Object.Instantiate(instantiate.SharedData);
        copyTables.Add(instantiate);
      }
      if (RegenerateTranslationsUtils.ifRegenerate)
        RegenerateTranslationsUtils.ConvertDirectoryIntoFiles();
      try
      {
        foreach (var localeAdded in LanguageController.AddedLocales)
        {
          foreach (var cloned in copyTables.Select(original => LanguageController.CreateClonedTable(original, localeAdded.Key, localeAdded.Value)))
          {
            LanguageController.CachedStringTables.Add(cloned.name, cloned);
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
    public static bool Patch_ProvideResource(ResourceManager __instance, IResourceLocation location, Type desiredType, ref AsyncOperationHandle __result)
    {
      if (location == null || !location.PrimaryKey.Contains("MODDEDLanguage/")) return true;
      
      if (LanguageController.CachedStringTables.TryGetValue(location.PrimaryKey.Replace("MODDEDLanguage/", string.Empty), out var value))
      {
        __result = __instance.CreateCompletedOperation(value, string.Empty);
        return false;
      }
      MelonLogger.Msg($"Can't find table: {location.PrimaryKey}, returning true");
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
        if (key.ToString().Contains($"_{locale.Identifier.Code}") && LanguageController.ResourceLocationBases.TryGetValue("MODDEDLanguage/" + key.ToString(), out var value))
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
