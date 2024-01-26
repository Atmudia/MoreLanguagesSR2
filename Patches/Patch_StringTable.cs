// Decompiled with JetBrains decompiler
// Type: MoreLanguagesMod.StringTableAddEntryPatch
// Assembly: MoreLanguagesMod, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 67967EAE-1BC3-437A-84DC-B4390CE82D77
// Assembly location: D:\SlimeRancherModding\SR2\TestingMod\MoreLanguagesMod.dll

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