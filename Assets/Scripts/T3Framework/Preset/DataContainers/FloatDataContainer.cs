#nullable enable

using System;
using T3Framework.Runtime.Event;
using UnityEngine;

namespace T3Framework.Preset.DataContainers
{
	public class FloatDataContainer : NotifiableDataContainer<float>
	{
		// Serializable and Public
		[SerializeField] private float initialValue;
		[SerializeField] private bool hasMinValue = false;
		[SerializeField] private float minValue;
		[SerializeField] private bool hasMaxValue = false;
		[SerializeField] private float maxValue;

		protected override Func<float, float> Clamp => x =>
			Mathf.Clamp(x, hasMinValue ? minValue : float.MinValue, hasMaxValue ? maxValue : float.MaxValue);

		protected override Func<float, float, bool> Comparer => Mathf.Approximately;

		public override float InitialValue => initialValue;
	}
}