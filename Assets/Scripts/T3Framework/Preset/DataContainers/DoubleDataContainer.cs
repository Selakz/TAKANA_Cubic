#nullable enable

using System;
using T3Framework.Runtime.Event;
using UnityEngine;

namespace T3Framework.Preset.DataContainers
{
	public class DoubleDataContainer : NotifiableDataContainer<double>
	{
		// Serializable and Public
		[SerializeField] private double initialValue;
		[SerializeField] private bool hasMinValue = false;
		[SerializeField] private double minValue;
		[SerializeField] private bool hasMaxValue = false;
		[SerializeField] private double maxValue;

		protected override Func<double, double> Clamp => value =>
		{
			if (hasMinValue && value < minValue) value = minValue;
			else if (hasMaxValue && value > maxValue) value = maxValue;
			return value;
		};

		protected override Func<double, double, bool> Comparer => (x, y) => Math.Abs(x - y) < double.Epsilon;

		public override double InitialValue => initialValue;
	}
}