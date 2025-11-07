#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;

namespace T3Framework.Runtime.Setting
{
	public struct SettingValueConverter
	{
		public Func<object, object> FromActualToDisplay { get; set; }
		public Func<object, object> FromDisplayToActual { get; set; }

		public SettingValueConverter(Func<object, object> fromActualToDisplay, Func<object, object> fromDisplayToActual)
		{
			FromActualToDisplay = fromActualToDisplay;
			FromDisplayToActual = fromDisplayToActual;
		}
	}

	public interface ISettingItem
	{
		public Type? SettingType { get; }

		public PropertyInfo? TargetPropertyInfo { get; }

		public string FullClassName { get; }

		public string PropertyName { get; }

		public void Initialize(string fullClassName, string propertyName);

		private static readonly Dictionary<string, SettingValueConverter> valueConverters = new();

		public static void SetValueConverter(string name, SettingValueConverter valueConverter)
		{
			valueConverters[name] = valueConverter;
		}

		public static SettingValueConverter? GetValueConverter(string name)
		{
			return valueConverters.GetValueOrDefault(name);
		}
	}
}