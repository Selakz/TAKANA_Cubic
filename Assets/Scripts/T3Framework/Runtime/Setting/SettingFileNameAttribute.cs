using System;

namespace T3Framework.Runtime.Setting
{
	[AttributeUsage(AttributeTargets.Class)]
	public class SettingFileNameAttribute : Attribute
	{
		public string FileName { get; set; }

		public SettingFileNameAttribute(string fileName)
		{
			FileName = fileName;
		}
	}
}