﻿/*
 * Copyright 2021 Peter Han
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
using System.Reflection;
using TMPro;
using UnityEngine;

namespace PeterHan.PLib.Options {
	/// <summary>
	/// An options entry which represents int? and displays a text field and slider.
	/// </summary>
	public class NullableIntOptionsEntry : SlidingBaseOptionsEntry {
		/// <summary>
		/// The text that is rendered for the current value of the entry.
		/// </summary>
		protected virtual string FieldText {
			get {
				return value?.ToString(Format ?? "D") ?? string.Empty;
			}
		}

		public override object Value {
			get {
				return value;
			}
			set {
				if (value == null) {
					this.value = null;
					Update();
				} else if (value is int newValue) {
					this.value = newValue;
					Update();
				}
			}
		}

		/// <summary>
		/// The realized text field.
		/// </summary>
		private GameObject textField;

		/// <summary>
		/// The value in the text field.
		/// </summary>
		private int? value;

		public NullableIntOptionsEntry(string field, IOptionSpec spec,
				LimitAttribute limit = null) : base(field, spec, limit) {
			textField = null;
			value = null;
		}

		protected override PSliderSingle GetSlider() {
			float minLimit = (float)limits.Minimum, maxLimit = (float)limits.Maximum;
			return new PSliderSingle() {
				OnValueChanged = OnSliderChanged, ToolTip = LookInStrings(Tooltip),
				MinValue = minLimit, MaxValue = maxLimit, InitialValue = minLimit,
				IntegersOnly = true
			};
		}

		public override GameObject GetUIComponent() {
			textField = new PTextField() {
				OnTextChanged = OnTextChanged, ToolTip = LookInStrings(Tooltip),
				Text = FieldText, MinWidth = 64, MaxLength = 10,
				Type = PTextField.FieldType.Integer
			}.Build();
			Update();
			return textField;
		}

		/// <summary>
		/// Called when the slider's value is changed.
		/// </summary>
		/// <param name="newValue">The new slider value.</param>
		private void OnSliderChanged(GameObject _, float newValue) {
			int newIntValue = Mathf.RoundToInt(newValue);
			if (limits != null)
				newIntValue = limits.ClampToRange(newIntValue);
			// Record the value
			value = newIntValue;
			Update();
		}

		/// <summary>
		/// Called when the input field's text is changed.
		/// </summary>
		/// <param name="text">The new text.</param>
		private void OnTextChanged(GameObject _, string text) {
			if (string.IsNullOrWhiteSpace(text))
				// Limits are assumed to allow null, because why not use non-nullable
				// otherwise?
				value = null;
			else if (int.TryParse(text, out int newValue)) {
				if (limits != null)
					newValue = limits.ClampToRange(newValue);
				// Record the valid value
				value = newValue;
			}
			Update();
		}

		/// <summary>
		/// Updates the displayed value.
		/// </summary>
		protected override void Update() {
			var field = textField?.GetComponentInChildren<TMP_InputField>();
			if (field != null)
				field.text = FieldText;
			if (slider != null && value != null)
				PSliderSingle.SetCurrentValue(slider, (int)value);
		}
	}
}
