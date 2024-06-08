using System.Collections;
using System.Linq;
using Il2Cpp;
using Il2CppMonomiPark.SlimeRancher.SceneManagement;
using Il2CppMonomiPark.SlimeRancher.Shop;
using MelonLoader;
using MoreLanguagesMod;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using Locale = UnityEngine.Localization.Locale;

[assembly: MelonInfo(typeof(EntryPoint), "MoreLanguagesMod", "1.0.6", "KomiksPL", "https://www.nexusmods.com/slimerancher2/mods/31")]
namespace MoreLanguagesMod
{
  
  internal class EntryPoint : MelonMod
  {
    public static System.Collections.Generic.List<StringTable> copyTables = new System.Collections.Generic.List<StringTable>();

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
    }

    public override void OnSceneWasInitialized(int buildIndex, string sceneName)
    {
      switch (sceneName)
      {
        case "SystemCore":
          break;
      }
    }
    

    public override void OnInitializeMelon()
    {
      LanguageController.Setup();
      LanguageController.InstallLocale(Locale.CreateLocale(SystemLanguage.Polish));
      // ShopItemAssetReference
      // LanguageController.InstallLocale(Locale.CreateLocale(SystemLanguage.Turkish));
    }

    public static void Spawn(GameObject obj)
    {
      InstantiationHelpers.InstantiateActor(obj,
        SystemContext.Instance.SceneLoader.CurrentSceneGroup, SceneContext.Instance.Player.transform.position,
        Quaternion.identity);

      
    }
  }
}
