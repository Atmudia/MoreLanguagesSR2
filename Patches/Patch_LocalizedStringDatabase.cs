using HarmonyLib;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace MoreLanguagesMod.Patches;

[HarmonyPatch(typeof(LocalizedStringDatabase))]
public static class Patch_LocalizedStringDatabase
{
    [HarmonyPatch(nameof(GenerateLocalizedString)), HarmonyPrefix]
    public static void GenerateLocalizedString(ref StringTable table, ref StringTableEntry entry, TableEntryReference tableEntryReference)
    {
        if (entry == null && table)
        {
            entry = table.GetEntryOrAddEntry(tableEntryReference.KeyId, tableEntryReference.Key);
        }
    }
}