using HarmonyLib;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Core.Formatting;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.Localization.Tables;

namespace MoreLanguagesMod.Patches;

[HarmonyPatch(typeof(LocalizedStringDatabase))]
public static class Patch_LocalizedStringDatabase
{
    [HarmonyPatch(nameof(GenerateLocalizedString)), HarmonyPrefix]
    public static void GenerateLocalizedString(ref StringTable table, ref StringTableEntry entry, TableReference tableReference, TableEntryReference tableEntryReference, Locale locale)
    {
        if (table != null || entry != null)
            return;
        (StringTable stringTable, StringTableEntry stringTableEntry) = LanguageController.ResetTranslations(tableReference, tableEntryReference, locale);
        if (stringTableEntry == null || stringTable == null)
            return;
        table = stringTable;
        entry = stringTableEntry;
        if (entry.m_FormatCache != null)
            return;
        entry.m_FormatCache = entry.GetOrCreateFormatCache() ?? new FormatCache();
        entry.m_FormatCache.LocalVariables = Patch_LocalizedString.Instance.TryCast<IVariableGroup>();
        entry.m_FormatCache.VariableTriggers.Clear();
        Patch_LocalizedString.Instance = null;
    }
}