using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine.Localization.Tables;

namespace MoreLanguagesMod.Patches
{
    [HarmonyPatch]
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
            if (!LanguageController.moddedTranslations.TryGetValue(instance.TableCollectionName, out var dictionary))
            {
                dictionary = new Dictionary<long, string>();
                LanguageController.moddedTranslations.TryAdd(instance.TableCollectionName, dictionary);
            }
            dictionary.TryAdd(keyId, localized);
        }
    }
}