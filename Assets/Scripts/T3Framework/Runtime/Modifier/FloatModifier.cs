#nullable enable

using System;

namespace T3Framework.Runtime.Modifier
{
	public class FloatMethod
	{
		public static FloatMethod Instance { get; } = new();
	}

	public class FloatModifier : BaseModifier<float, FloatMethod>
	{
		public FloatModifier(Func<float> getter, Action<float> setter, Func<float, float> resetFunction)
			: base(getter, setter, resetFunction)
		{
		}

		public FloatModifier(Func<float> getter, Action<float> setter, float defaultValue)
			: base(getter, setter, defaultValue)
		{
		}

		public FloatModifier(Func<float> getter, Action<float> setter)
			: base(getter, setter)
		{
		}

		protected override float ModifyStep(FloatMethod method, float lastValue, float thisValue)
		{
			if (ReferenceEquals(method, FloatMethod.Instance)) return thisValue;
			// TODO: the specific step
			return thisValue;
		}

		public override void Assign(float value, int priority) => Register(FloatMethod.Instance, value, priority);
	}
}