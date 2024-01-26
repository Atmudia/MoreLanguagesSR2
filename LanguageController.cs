// Decompiled with JetBrains decompiler
// Type: MoreLanguagesMod.LanguageController
// Assembly: MoreLanguagesMod, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 67967EAE-1BC3-437A-84DC-B4390CE82D77
// Assembly location: D:\SlimeRancherModding\SR2\TestingMod\MoreLanguagesMod.dll

using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppTMPro;
using MelonLoader;
using MelonLoader.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using Object = Il2CppSystem.Object;

namespace MoreLanguagesMod
{
  public static class LanguageController
  {
    private static TMP_FontAsset RusselType;
    private static TMP_FontAsset Nunito;
    private static TMP_FontAsset HumanSans;
    private static TMP_FontAsset NunitoLight;
    internal static List<Locale> addedLocales = new List<Locale>();
    public static Dictionary<string, Dictionary<long, string>> moddedTranslations = new Dictionary<string, Dictionary<long, string>>();
    internal static List<StringTable> cachedStringTables = new List<StringTable>();

    internal static void Setup()
    {
      Stream manifestResourceStream = Melon<EntryPoint>.Instance.MelonAssembly.Assembly.GetManifestResourceStream("MoreLanguagesMod.morelanguages");
      if (manifestResourceStream != null)
      {
        byte[] buffer = new byte[manifestResourceStream.Length];
        _ = manifestResourceStream.Read((System.Span<byte>) buffer);
        AssetBundle assetBundle = AssetBundle.LoadFromMemory(buffer);
        RusselType = TMP_FontAsset.CreateFontAsset(assetBundle.LoadAsset("KatahdinRound").Cast<Font>());
        RusselType.hideFlags |= HideFlags.HideAndDontSave;
        RusselType.name = "KatahdinRound";
        Nunito = TMP_FontAsset.CreateFontAsset(assetBundle.LoadAsset("Nunito-Bold").Cast<Font>());
        Nunito.hideFlags |= HideFlags.HideAndDontSave;
        Nunito.name = "Nunito-Bold";
        HumanSans = TMP_FontAsset.CreateFontAsset(assetBundle.LoadAsset("HumanSans-Regular").Cast<Font>());
        HumanSans.hideFlags |= HideFlags.HideAndDontSave;
        HumanSans.name = "HumanSans-Regular";
        NunitoLight = TMP_FontAsset.CreateFontAsset(assetBundle.LoadAsset("NunitoSans-Light").Cast<Font>());
        NunitoLight.hideFlags |= HideFlags.HideAndDontSave;
        NunitoLight.name = "NunitoSans-Light";
      }
    
    }

    internal static void InstallHemispheres(TextMeshProUGUI textMeshPro)
    {
      if (!textMeshPro.font.m_FallbackFontAssetTable.Contains(RusselType))
      {
        textMeshPro.font.m_FallbackFontAssetTable.Add(RusselType);
        RusselType = null;
      }
    }

    internal static void InstallHumanstSDF(TextMeshProUGUI textMeshPro)
    {
      if (!textMeshPro.font.m_FallbackFontAssetTable.Contains(HumanSans))
      {
        textMeshPro.font.m_FallbackFontAssetTable.Add(HumanSans);
        HumanSans = null;
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
      if (!Directory.Exists(Path.Combine(MelonEnvironment.MelonBaseDirectory, "MoreLanguagesMod", code)))
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
