using System.Reflection;
using HarmonyLib;
using UnityEngine.Localization.Tables;

namespace MoreLanguagesMod.Patches
{
    [HarmonyPatch]
    // [HarmonyPatch(typeof(DetailedLocalizationTable<StringTableEntry>), "AddEntry", typeof(string), typeof(string))]
    public static class Patch_StringTable
    {
        public static MethodBase TargetMethod() => typeof (DetailedLocalizationTable<>).MakeGenericType(typeof (StringTableEntry)).GetMethod("AddEntry", new Type[2]
        {
            typeof (string),
            typeof (string)
        });

        public static void Postfix(object __instance, string key, string localized)
        {
            if (__instance is not DetailedLocalizationTable<StringTableEntry> instance) return;
            
            long keyId = instance.FindKeyId(key, false);
            if (keyId == 0L)
                return;
            if (!LanguageController.ModdedTranslations.TryGetValue(instance.TableCollectionName, out var dictionary))
            {
               
                LanguageController.ModdedTranslations.TryAdd(instance.TableCollectionName, dictionary = new Dictionary<long, string>());
            }
            dictionary.TryAdd(keyId, localized);
        }
    }
}