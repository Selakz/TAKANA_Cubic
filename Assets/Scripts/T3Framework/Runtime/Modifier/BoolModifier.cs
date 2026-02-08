#nullable enable

using System;

namespace T3Framework.Runtime.Modifier
{
	public enum BoolMethod
	{
		Assign,
		And,
		Or
	}

	public class BoolModifier : BaseModifier<bool, BoolMethod>
	{
		public BoolModifier(Func<bool> getter, Action<bool> setter, Func<bool, bool> resetFunction)
			: base(getter, setter, resetFunction)
		{
		}

		public BoolModifier(Func<bool> getter, Action<bool> setter, bool defaultValue)
			: base(getter, setter, defaultValue)
		{
		}

		public BoolModifier(Func<bool> getter, Action<bool> setter)
			: base(getter, setter)
		{
		}

		protected override bool ModifyStep(BoolMethod method, bool lastValue, bool thisValue)
		{
			return method switch
			{
				BoolMethod.Assign => thisValue,
				BoolMethod.And => lastValue && thisValue,
				BoolMethod.Or => lastValue || thisValue,
				_ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
			};
		}

		public override void Assign(bool value, int priority) => Register(BoolMethod.Assign, value, priority);
	}
}