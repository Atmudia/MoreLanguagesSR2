using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTMPro;
using MelonLoader;
using MelonLoader.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Il2CppInterop.Runtime;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using Object = UnityEngine.Object;

namespace MoreLanguagesMod
{
  public static class LanguageController
  {
    internal static List<Locale> AddedLocales = new List<Locale>();
    internal static Dictionary<string, Dictionary<long, string>> ModdedTranslations = new Dictionary<string, Dictionary<long, string>>();
    internal static readonly Dictionary<string, StringTable> CachedStringTables = new Dictionary<string, StringTable>();
    
    private static readonly Dictionary<string, TMP_FontAsset> FontAssets = new Dictionary<string, TMP_FontAsset>();
    internal static Dictionary<string, Il2CppSystem.Collections.Generic.IList<IResourceLocation>> ResourceLocationBases { get; } = new Dictionary<string, Il2CppSystem.Collections.Generic.IList<IResourceLocation>>();

    internal static void Setup()
    {
      Stream manifestResourceStream = Melon<EntryPoint>.Instance.MelonAssembly.Assembly.GetManifestResourceStream("MoreLanguagesMod.morelanguages");
      if (manifestResourceStream != null)
      {
        byte[] buffer = new byte[manifestResourceStream.Length];
        _ = manifestResourceStream.Read((System.Span<byte>) buffer);
        AssetBundle assetBundle = AssetBundle.LoadFromMemory(buffer);
        var enumerable = assetBundle.LoadAllAssets(Il2CppType.Of<Font>()).Select(x => x.Cast<Font>());
        foreach (var font in enumerable)
        {
          var tmpFontAsset = TMP_FontAsset.CreateFontAsset(font);
          tmpFontAsset.hideFlags |= HideFlags.HideAndDontSave;
          tmpFontAsset.name = font.name;
          FontAssets.Add(tmpFontAsset.name, tmpFontAsset);
        }
      }
    
    }

    internal static void InstallHemispheres(TextMeshProUGUI textMeshPro)
    {
      var tmpFontAsset = FontAssets["KatahdinRound"];
      if (!textMeshPro.font.m_FallbackFontAssetTable.Contains(tmpFontAsset))
      {
        textMeshPro.font.m_FallbackFontAssetTable.Add(tmpFontAsset);
      }
    }
    

    internal static void InstallLexend(TextMeshProUGUI textMeshPro)
    {
      if (LanguageController.FontAssets.TryGetValue(textMeshPro.font.name.Replace(" (Latin)", string.Empty), out var result))
      {
        if (!textMeshPro.font.m_FallbackFontAssetTable.Contains(result))
        {
          textMeshPro.font.m_FallbackFontAssetTable.Add(result);
        }
      }
     
    }

    public static void InstallLocale(Locale locale)
    {
      locale.name = locale.Identifier.ToString();
      Locale locale1 = locale;
      locale1.hideFlags |= HideFlags.HideAndDontSave;
      AddedLocales.Add(locale);
    }

    internal static StringTableEntry GetEntryOrAddEntry(
      this StringTable stringTable,
      long keyId,
      string key)
    {
      StringTableEntry entryOrAddEntry = null;
      try
      {
        if (keyId != 0L)
          entryOrAddEntry = stringTable.GetEntry(keyId);
        else if (!string.IsNullOrWhiteSpace(key))
          entryOrAddEntry = stringTable.GetEntry(key);
      }
      catch
      {
        // ignored
      }

      if (entryOrAddEntry != null)
        return entryOrAddEntry;
      return ModdedTranslations.TryGetValue(stringTable.TableCollectionName, out var dictionary) && dictionary.TryGetValue(keyId, out var str) ? stringTable.AddEntry(keyId, str) : null;
    }

    public static void AddResourceLocationsForLocale(Locale locale)
    {
      foreach (var tableCollectionName in EntryPoint.copyTables.Select(x => x.TableCollectionName))
      {
        var key = $"MODDEDLanguage/{tableCollectionName}_{locale.Identifier.Code}";
        var resourceLocation =
          new ResourceLocationBase(key, key, typeof(BundledAssetProvider).FullName, Il2CppType.Of<Object>());
        var list = new Il2CppSystem.Collections.Generic.List<IResourceLocation>();
        list.Add(resourceLocation.Cast<IResourceLocation>());
        ResourceLocationBases.Add(key, list.Cast<Il2CppSystem.Collections.Generic.IList<IResourceLocation>>());
      }
    }
    public static StringTable CreateClonedTable(StringTable original, Locale localeAdded)
    {
      var cloned = Object.Instantiate(original);
      cloned.name = original.name.Replace("_en(Clone)", $"_{localeAdded.Identifier.Code}");
      cloned.LocaleIdentifier = localeAdded.Identifier;
      cloned.hideFlags |= HideFlags.HideAndDontSave;
      
      var path = Path.Combine(MelonEnvironment.MelonBaseDirectory, "MoreLanguages", localeAdded.Identifier.Code, $"{original.SharedData.TableCollectionName}.json");
      if (File.Exists(path))
      {
        var translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
        foreach (var entry in cloned.m_TableEntries)
        {
          if (!string.IsNullOrEmpty(entry.Value.Key) && translations.TryGetValue(entry.Value.Key, out var value))
          {
            entry.Value.Value = value;
          }
        }
      }

      return cloned;
    }


    // internal static (StringTable, StringTableEntry) ResetTranslations(
      //   TableReference tableReference,
      //   TableEntryReference tableEntryReference,
      //   Locale locale)
      // {
      //   string code = locale.Identifier.Code;
      //   var combine = Path.Combine(MelonEnvironment.MelonBaseDirectory, "MoreLanguages", code);
      //   if (!Directory.Exists(combine))
      //   {
      //     MelonLogger.Msg("The language '" + locale.name + "' doesn't have a related folder for the language. LangCode: " + code);
      //     return default;
      //   }
      //   StringTable stringTable = cachedStringTables.FirstOrDefault((x => x.name.Contains(tableReference.TableCollectionName) && Object.Equals(x.LocaleIdentifier, locale.Identifier)));
      //   if (stringTable == null)
      //     return default;
      //   StringTableEntry entryOrAddEntry = stringTable.GetEntryOrAddEntry(tableEntryReference.KeyId, tableEntryReference.Key);
      //   return (stringTable, entryOrAddEntry);
      // }
  }
}
