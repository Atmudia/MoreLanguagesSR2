using System;
using HarmonyLib;
using MelonLoader;

namespace MoreLanguagesMod.Patches
{
	[HarmonyPatch("Il2CppInterop.HarmonySupport.Il2CppDetourMethodPatcher", "ReportException")]
	public static class Patch_Il2CppDetourMethodPatcher
	{
		public static bool Prefix(Exception ex)
		{
			MelonLogger.Error("During invoking native->managed trampoline", ex);
			return false;                               
		}
	}
}
