using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.Core.Formatting;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Localization.Tables;

namespace MoreLanguagesMod.Patches;

[HarmonyPatch(typeof(LocalizedString))]
public static class Patch_LocalizedString
{ 
    internal static LocalizedString Instance;
     
    [HarmonyPatch(nameof(GetSourceValue)), HarmonyPrefix]
    public static bool GetSourceValue(LocalizedString __instance, ISelectorInfo selector, ref object __result)
    {
      if (__instance.IsEmpty)
      {
        __result = string.Empty;
        return true;
      }
      Locale locale = __instance.LocaleOverride;
      if (locale == null && selector.FormatDetails.FormatCache != null)
        locale = LocalizationSettings.AvailableLocales.GetLocale(selector.FormatDetails.FormatCache.Table.LocaleIdentifier);
      if (locale == null && LocalizationSettings.SelectedLocaleAsync.IsDone)
        locale = LocalizationSettings.SelectedLocaleAsync.Result;
      if (locale == null)
      {
        __result = "<No Available Locale>";
        return true;
      }
      if (LanguageController.addedLocales.FirstOrDefault( x => x.Identifier.Code == locale.Identifier.Code) == null)
        return true;
      StringTable stringTable = LanguageController.cachedStringTables.Find(x => x.TableCollectionName == __instance.TableReference.TableCollectionName);
      StringTableEntry stringTableEntry = stringTable.GetEntry(__instance.TableEntryReference.KeyId) ?? stringTable.GetEntry(__instance.TableEntryReference.Key);
      LocalizedStringDatabase stringDatabase = LocalizationSettings.StringDatabase;
      TableReference tableReference = __instance.TableReference;
      TableEntryReference tableEntryReference = __instance.TableEntryReference;
      if (!stringTableEntry.IsSmart)
      {
        __result = Il2CppSystem.Activator.CreateInstance(Il2CppType.Of<LocalizedString.StringTableEntryVariable>(), stringDatabase.GenerateLocalizedString(stringTable, stringTableEntry, tableReference, tableEntryReference, locale, __instance.Arguments), stringTableEntry);
        return false;
      }
      FormatCache formatCache = stringTableEntry?.GetOrCreateFormatCache();
      if (formatCache != null)
      {
        formatCache.VariableTriggers.Clear();
        formatCache.LocalVariables = __instance.m_VariableLookup.Count <= 0 ? selector.FormatDetails.FormatCache.LocalVariables : new LocalizedString.ChainedLocalVariablesGroup(__instance.Cast<IVariableGroup>(), selector.FormatDetails.FormatCache.LocalVariables).Cast<IVariableGroup>();
      }

      Il2CppSystem.Collections.Generic.List<Il2CppSystem.Object> list = new Il2CppSystem.Collections.Generic.List<Il2CppSystem.Object>();
      if (selector.CurrentValue != null)
        list.Add(selector.CurrentValue);
      if (__instance.Arguments != null)
        list.AddRange(__instance.Arguments.Cast<Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Object>>());
      string localizedString = stringDatabase.GenerateLocalizedString(stringTable, stringTableEntry, tableReference, tableEntryReference, locale, list.Cast<Il2CppSystem.Collections.Generic.IList<Il2CppSystem.Object>>());
      if (formatCache != null)
      {
        formatCache.LocalVariables = null;
        __instance.UpdateVariableListeners(formatCache.VariableTriggers);
      }
      __result = Il2CppSystem.Activator.CreateInstance(Il2CppType.Of<LocalizedString.StringTableEntryVariable>(), localizedString, stringTableEntry);
      return false;
    }

    [HarmonyPatch(nameof(RefreshString)), HarmonyPrefix]
    public static void RefreshString(LocalizedString __instance) => Instance = __instance;
}