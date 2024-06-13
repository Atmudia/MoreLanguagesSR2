using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using HarmonyLib;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Il2CppMonomiPark.SlimeRancher.Shop;
using Il2CppMonomiPark.SlimeRancher.UI.Popup;
using MelonLoader;
using MelonLoader.Utils;
using MoreLanguagesMod;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Locale = UnityEngine.Localization.Locale;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(EntryPoint), "MoreLanguagesMod", "1.0.7", "KomiksPL", "https://www.nexusmods.com/slimerancher2/mods/31")]
namespace MoreLanguagesMod
{
  [HarmonyPatch]
  
  internal class EntryPoint : MelonMod
  {
    [DllImport("User32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBox(IntPtr h, string m, string c, int type);
    public static System.Collections.Generic.List<StringTable> copyTables = new System.Collections.Generic.List<StringTable>();
    public static bool Activated = false;

    public static IEnumerator GetAllTables(Locale locale)
    {
      AsyncOperationHandle<Il2CppSystem.Collections.Generic.IList<StringTable>> asyncOperationHandle = LocalizationSettings.StringDatabase.GetAllTables(locale);
      yield return asyncOperationHandle;
      Il2CppSystem.Collections.Generic.List<StringTable> list = new Il2CppSystem.Collections.Generic.List<StringTable>(asyncOperationHandle.Result.Cast<Il2CppSystem.Collections.Generic.IEnumerable<StringTable>>());
      foreach (StringTable stringTable1 in list)
      {
        if (RegenerateTranslationsUtils.ifRegenerate)
          RegenerateTranslationsUtils.AddItToList(stringTable1);
        StringTable instantiate = UnityEngine.Object.Instantiate(stringTable1);
        instantiate.hideFlags |= HideFlags.HideAndDontSave;
        instantiate.SharedData = UnityEngine.Object.Instantiate(instantiate.SharedData);
        copyTables.Add(instantiate);
      }
      if (RegenerateTranslationsUtils.ifRegenerate)
        RegenerateTranslationsUtils.ConvertDirectoryIntoFiles();
      try
      {
        foreach (var localeAdded in LanguageController.AddedLocales)
        {
          foreach (var cloned in copyTables.Select(original => LanguageController.CreateClonedTable(original, localeAdded)))
          {
            LanguageController.CachedStringTables.Add(cloned.name, cloned);
          }

          LanguageController.AddResourceLocationsForLocale(localeAdded);
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
      if (!EntryPoint.Activated)
        return true;
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
    public static bool LoadResourceLocationsAsync(Il2CppSystem.Object key, ref AsyncOperationHandle<Il2CppSystem.Collections.Generic.IList<IResourceLocation>> __result)
    {
      if (!EntryPoint.Activated)
        return true;
      
      if (key == null)
        return true;
      foreach (var locale in LanguageController.AddedLocales)
      {
        MelonLogger.Msg(key.ToString());
        if (key.ToString()!.Contains($"_{locale.Identifier.Code}") && LanguageController.ResourceLocationBases.TryGetValue("MODDEDLanguage/" + key.ToString(), out var value))
        {
          __result = Addressables.ResourceManager.CreateCompletedOperation(value, string.Empty);
          MelonLogger.Msg($"Setting custom ResourceLocation: {value}");
          return false;
        }      
      }
      return true;
            
    }
    
    

    public override void OnInitializeMelon()
    {

      if (!Directory.Exists(Path.Combine(MelonEnvironment.MelonBaseDirectory, "MoreLanguages")))
      {
        Activated = false;
        MessageBox(IntPtr.Zero, "The MoreLanguagesMod is not installed properly. Please read the mod description again", "Information", 0);
        return;
      }

      LanguageController.Setup();
      LanguageController.InstallLocale(Locale.CreateLocale(SystemLanguage.Polish));



      // ShopItemAssetReference
      // LanguageController.InstallLocale(Locale.CreateLocale(SystemLanguage.Turkish));
    }
    
  }
}
