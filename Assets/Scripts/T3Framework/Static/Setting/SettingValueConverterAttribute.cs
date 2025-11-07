#nullable enable

using System;

namespace T3Framework.Static.Setting
{
	[AttributeUsage(AttributeTargets.Property)]
	public class SettingValueConverterAttribute : Attribute
	{
		public Type ToType { get; set; }

		public string ConverterName { get; set; }

		public SettingValueConverterAttribute(Type toType, string converterName)
		{
			ToType = toType;
			ConverterName = converterName;
		}
	}
}