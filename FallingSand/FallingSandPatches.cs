﻿/*
 * Copyright 2019 Peter Han
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
using System.Collections.Generic;
using UnityEngine;

namespace PeterHan.FallingSand {
	/// <summary>
	/// Patches which will be applied via annotations for Falling Sand.
	/// </summary>
	public static class FallingSandPatches {
		/// <summary>
		/// Checks a falling object to see if a dig errand must be placed.
		/// </summary>
		/// <param name="obj">The object which is falling.</param>
		private static void CheckFallingObject(GameObject obj) {
			var dig = obj.GetComponent<FallFromDigging>();
			if (dig != null) {
				int cell = Grid.PosToCell(obj.transform.GetPosition());
				if (Grid.IsValidCell(cell) && Grid.IsVisible(cell)) {
					// Did it land somewhere visible?
					int below = Grid.CellBelow(cell);
					if (Grid.IsValidCell(below) && (Grid.Element[below].IsSolid ||
							(Grid.Properties[below] & 4) != 0))
						FallingSandManager.Instance.QueueDig(cell, dig.Priority);
				}
			}
		}

		public static void OnLoad() {
			PUtil.InitLibrary();
		}

		/// <summary>
		/// Applied to Diggable to add a tracking component to objects which fall when
		/// digging.
		/// </summary>
		[HarmonyPatch(typeof(Diggable), "OnWorkTick")]
		public static class Diggable_OnWorkTick_Patch {
			internal static void Postfix(ref Diggable __instance) {
				FallingSandManager.Instance.TrackDiggable(__instance);
			}
		}

		/// <summary>
		/// Applied to Diggable to stop tracking digs which are destroyed.
		/// </summary>
		[HarmonyPatch(typeof(Diggable), "OnCleanUp")]
		public static class Diggable_OnCleanUp_Patch {
			internal static void Postfix(ref Diggable __instance) {
				FallingSandManager.Instance.UntrackDiggable(__instance);
			}
		}

		/// <summary>
		/// Applied to Game to stop tracking all digging errands.
		/// </summary>
		[HarmonyPatch(typeof(Game), "OnDestroy")]
		public static class Game_OnDestroy_Patch {
			internal static void Postfix() {
				FallingSandManager.Instance.ClearAll();
			}
		}

		/// <summary>
		/// Applied to UnstableGroundManager to flag spawned falling objects with the
		/// appropriate component.
		/// </summary>
		[HarmonyPatch(typeof(UnstableGroundManager), "Spawn", typeof(int), typeof(Element),
			typeof(float), typeof(float), typeof(byte), typeof(int))]
		public static class UnstableGroundManager_Spawn_Patch {
			internal static void Postfix(ref List<GameObject> ___fallingObjects, int cell) {
				int n = ___fallingObjects.Count;
				GameObject obj;
				Diggable cause;
				if (n > 0 && (obj = ___fallingObjects[n - 1].gameObject) != null) {
					// Actually caused by digging?
					if ((cause = FallingSandManager.Instance.FindDigErrand(cell)) != null &&
							cause.gameObject != null) {
						// Should never be null since object was just spawned
						var component = obj.AddComponent<FallFromDigging>();
						var xy = Grid.CellToXY(cell);
						PUtil.LogDebug("Digging induced: {0} @ ({1:D},{2:D})".F(obj.name,
							xy.X, xy.Y));
						// Unity equals operator strikes again
						var digPri = cause.gameObject.GetComponent<Prioritizable>();
						if (digPri != null)
							component.Priority = digPri.GetMasterPriority();
					}
				}
			}
		}

		/// <summary>
		/// Applied to UnstableGroundManager to actually place the digs, now that the blocks
		/// are solidified.
		/// </summary>
		[HarmonyPatch(typeof(UnstableGroundManager), "RemoveFromPending")]
		public static class UnstableGroundManager_RemoveFromPending_Patch {
			internal static void Postfix(int cell, ref List<int> ___pendingCells) {
				FallingSandManager.Instance.CheckDigQueue(cell);
				if (___pendingCells.Count < 1)
					FallingSandManager.Instance.ClearDig();
			}
		}

		/// <summary>
		/// Applied to UnstableGroundManager to queue up dig errands on falling objects which
		/// are about to become solid.
		/// </summary>
		[HarmonyPatch(typeof(UnstableGroundManager), "Update")]
		public static class UnstableGroundManager_Update_Patch {
			internal static void Prefix(ref List<GameObject> ___fallingObjects) {
				foreach (var obj in ___fallingObjects)
					if (obj != null)
						CheckFallingObject(obj);
			}
		}
	}
}
