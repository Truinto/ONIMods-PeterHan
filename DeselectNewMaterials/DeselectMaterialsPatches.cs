﻿/*
 * Copyright 2020 Peter Han
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using Harmony;
using PeterHan.PLib;
using PeterHan.PLib.Options;
using UnityEngine;

namespace PeterHan.DeselectNewMaterials {
	/// <summary>
	/// Patches for Deselect New Materials.
	/// </summary>
	public sealed class DeselectMaterialsPatches {
		/// <summary>
		/// The options for this mod.
		/// </summary>
		private static DeselectMaterialsOptions options;

		public static void OnLoad() {
			PUtil.InitLibrary();
			options = new DeselectMaterialsOptions();
			POptions.RegisterOptions(typeof(DeselectMaterialsOptions));
		}

		/// <summary>
		/// Applied to Game to load settings when the mod starts up.
		/// </summary>
		[HarmonyPatch(typeof(Game), "OnPrefabInit")]
		public static class Game_OnPrefabInit_Patch {
			/// <summary>
			/// Applied before OnPrefabInit runs.
			/// </summary>
			internal static void Prefix() {
				options = POptions.ReadSettings<DeselectMaterialsOptions>() ??
					new DeselectMaterialsOptions();
				PUtil.LogDebug("DeselectNewMaterials settings: Ignore Food = {0}".F(options.
					IgnoreFoodBoxes));
			}
		}

		/// <summary>
		/// Applied to TreeFilterable to deselect any new item discovered.
		/// </summary>
		[HarmonyPatch(typeof(TreeFilterable), "OnDiscover")]
		public static class TreeFilterable_OnDiscover_Patch {
			/// <summary>
			/// Applied after OnDiscover runs.
			/// </summary>
			internal static void Postfix(TreeFilterable __instance, Storage ___storage,
					Tag category_tag, Tag tag) {
				GameObject obj;
				if ((obj = __instance.gameObject) != null) {
					// Ignore fridges and ration boxes if the check box is set
					bool isFood = obj.GetComponent<Refrigerator>() != null || obj.
						GetComponent<RationBox>() != null;
					if (obj != null && (!options.IgnoreFoodBoxes || !isFood) && ___storage.
							storageFilters.Contains(category_tag))
						__instance.RemoveTagFromFilter(tag);
				}
			}
		}
	}
}
