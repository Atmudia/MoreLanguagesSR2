using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTMPro;
using MelonLoader;
using MelonLoader.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using Object = Il2CppSystem.Object;

namespace MoreLanguagesMod
{
  public static class LanguageController
  {
    internal static List<Locale> addedLocales = new List<Locale>();
    public static Dictionary<string, Dictionary<long, string>> moddedTranslations = new Dictionary<string, Dictionary<long, string>>();
    internal static List<StringTable> cachedStringTables = new List<StringTable>();


    public static Dictionary<string, TMP_FontAsset> FontAssets = new Dictionary<string, TMP_FontAsset>();

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
        
        // RusselType = TMP_FontAsset.CreateFontAsset(assetBundle.LoadAsset("KatahdinRound").Cast<Font>());
        // RusselType.hideFlags |= HideFlags.HideAndDontSave;
        // RusselType.name = "KatahdinRound";
        // Nunito = TMP_FontAsset.CreateFontAsset(assetBundle.LoadAsset("Nunito-Bold").Cast<Font>());
        // Nunito.hideFlags |= HideFlags.HideAndDontSave;
        // Nunito.name = "Nunito-Bold";
        // HumanSans = TMP_FontAsset.CreateFontAsset(assetBundle.LoadAsset("HumanSans-Regular").Cast<Font>());
        // HumanSans.hideFlags |= HideFlags.HideAndDontSave;
        // HumanSans.name = "HumanSans-Regular";
        // NunitoLight = TMP_FontAsset.CreateFontAsset(assetBundle.LoadAsset("NunitoSans-Light").Cast<Font>());
        // NunitoLight.hideFlags |= HideFlags.HideAndDontSave;
        // NunitoLight.name = "NunitoSans-Light";
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
      MelonLogger.Msg(textMeshPro.font.name);
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
      addedLocales.Add(locale);
    }

    private static StringTableEntry GetEntryOrAddEntry(
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
      return moddedTranslations.TryGetValue(stringTable.TableCollectionName, out var dictionary) && dictionary.TryGetValue(keyId, out var str) ? stringTable.AddEntry(keyId, str) : null;
    }

    internal static (StringTable, StringTableEntry) ResetTranslations(
      TableReference tableReference,
      TableEntryReference tableEntryReference,
      Locale locale)
    {
      string code = locale.Identifier.Code;
      var combine = Path.Combine(MelonEnvironment.MelonBaseDirectory, "MoreLanguages", code);
      if (!Directory.Exists(combine))
      {
        MelonLogger.Msg("The language '" + locale.name + "' doesn't have a related folder for the language. LangCode: " + code);
        return default;
      }
      StringTable stringTable = cachedStringTables.FirstOrDefault((x => x.name.Contains(tableReference.TableCollectionName) && Object.Equals(x.LocaleIdentifier, locale.Identifier)));
      if (stringTable == null)
        return default;
      StringTableEntry entryOrAddEntry = stringTable.GetEntryOrAddEntry(tableEntryReference.KeyId, tableEntryReference.Key);
      return (stringTable, entryOrAddEntry);
    }
  }
}
