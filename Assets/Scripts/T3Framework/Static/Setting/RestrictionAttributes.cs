#nullable enable

using System;

namespace T3Framework.Static.Setting
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MinValueAttribute : Attribute
	{
		public float MinValue { get; }

		public MinValueAttribute(float minValue)
		{
			MinValue = minValue;
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class MaxValueAttribute : Attribute
	{
		public float MaxValue { get; }

		public MaxValueAttribute(float minValue)
		{
			MaxValue = minValue;
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class MaxLengthAttribute : Attribute
	{
		public int MaxLength { get; }

		public MaxLengthAttribute(int maxLength)
		{
			MaxLength = maxLength;
		}
	}
}