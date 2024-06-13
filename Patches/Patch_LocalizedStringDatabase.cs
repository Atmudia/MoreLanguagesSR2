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
        if (entry == null && table)
        {
            entry = table.GetEntryOrAddEntry(tableEntryReference.KeyId, tableEntryReference.Key);
        }
    }
}