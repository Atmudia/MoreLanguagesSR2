using Il2CppTMPro;
using MelonLoader;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Il2CppInterop.Runtime;
// using MelonLoader.TinyJSON;
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
    internal static Dictionary<Locale, Assembly> AddedLocales = new Dictionary<Locale, Assembly>();
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
      if (!LanguageController.FontAssets.TryGetValue(textMeshPro.font.name.Replace(" (Latin)", string.Empty),
            out var result))
        result = FontAssets.FirstOrDefault(x => x.Key.Contains("Lexend")).Value;
      if (!textMeshPro.font.m_FallbackFontAssetTable.Contains(result))
      {
        textMeshPro.font.m_FallbackFontAssetTable.Add(result);
      }

    }

    public static void InstallLocale(Locale locale, Assembly assembly)
    {
      locale.name = locale.Identifier.ToString();
      locale.hideFlags |= HideFlags.HideAndDontSave;
      AddedLocales.Add(locale, assembly);
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
    public static StringTable CreateClonedTable(StringTable original, Locale localeAdded, Assembly assembly)
    {
      var cloned = Object.Instantiate(original);
      cloned.name = original.name.Replace("_en(Clone)", $"_{localeAdded.Identifier.Code}", StringComparison.InvariantCultureIgnoreCase);
      cloned.LocaleIdentifier = localeAdded.Identifier;
      cloned.hideFlags |= HideFlags.HideAndDontSave;
      
      
      string assemblyName = assembly.GetName().Name;
      string resourceName = $"{assemblyName}.LangFiles.{localeAdded.Identifier.Code}.{original.SharedData.TableCollectionName}.json";
      using Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
      if (resourceStream == null) return cloned;
      using StreamReader reader = new StreamReader(resourceStream);
      string jsonContent = reader.ReadToEnd();
      // var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
      // var translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
      JsonObject translations = (JsonObject)JsonNode.Parse(jsonContent);

      foreach (var entry in cloned.m_TableEntries)
      {
        if (!string.IsNullOrEmpty(entry.Value.Key) && translations.TryGetPropertyValue(entry.Value.Key, out var value))
        {
          entry.Value.Value = value.ToString();
        }
      }

      return cloned;
    }
  }
}
