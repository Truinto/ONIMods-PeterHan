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

using PeterHan.PLib.UI;
using System;
using System.Reflection;
using UnityEngine;

namespace PeterHan.PLib.Options {
	/// <summary>
	/// An options entry that displays a button. Not intended to be serializable to the
	/// options file, instead declare a read-only property that returns a handler method as
	/// an Action in the settings class, e.g:
	/// 
	/// [Option("Click Here!", "Button tool tip")]
	/// public System.Action MyButton => Handler;
	/// 
	/// public void Handler() {
	///     // ...
	/// }
	/// </summary>
	internal sealed class ButtonOptionsEntry : OptionsEntry {
		protected override object Value {
			get {
				return value;
			}
			set {
				if (value is System.Action newValue)
					this.value = newValue;
			}
		}

		/// <summary>
		/// The action to invoke when the button is pushed.
		/// </summary>
		private System.Action value;

		internal ButtonOptionsEntry(OptionAttribute oa, PropertyInfo prop) : base(prop?.Name,
				oa) { }

		internal override void CreateUIEntry(PGridPanel parent, int row) {
			parent.AddChild(new PButton(Field) {
				Text = LookInStrings(Title), ToolTip = LookInStrings(ToolTip),
				OnClick = OnButtonClicked
			}.SetKleiPinkStyle(), new GridComponentSpec(row, 0) {
				Margin = CONTROL_MARGIN, Alignment = TextAnchor.MiddleCenter, ColumnSpan = 2
			});
		}

		private void OnButtonClicked(GameObject _) {
			value?.Invoke();
		}

		protected override IUIComponent GetUIComponent() {
			// Will not be invoked
			return new PSpacer();
		}
	}
}
