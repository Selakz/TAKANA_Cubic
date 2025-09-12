using System;
using System.IO;
using UnityEngine;

namespace T3Framework.Runtime.Setting
{
	/// <summary> If the instance does not exist, this method will create a default file on it. </summary>
	public interface ISingletonSetting<out T> : ISetting<T> where T : ISingletonSetting<T>, new()
	{
		public static T Instance { get; } = new Lazy<T>(FromFile).Value;

		public static T FromFile()
		{
			var settingPath = Path.Combine(Application.persistentDataPath, "Settings");
			if (!Directory.Exists(settingPath)) Directory.CreateDirectory(settingPath);
			var filePath = Path.Combine(settingPath, GetFileName());
			if (File.Exists(filePath))
			{
				var yaml = File.ReadAllText(filePath);
				return SettingConvertHelper.Deserializer.Deserialize<T>(yaml);
			}

			var instance = new T();
			var text = SettingConvertHelper.Serializer.Serialize(instance);
			File.WriteAllText(filePath, text);
			return instance;
		}

		public static void SaveInstance()
		{
			var settingPath = Path.Combine(Application.persistentDataPath, "Settings");
			if (!Directory.Exists(settingPath)) Directory.CreateDirectory(settingPath);
			var text = SettingConvertHelper.Serializer.Serialize(Instance);
			var filePath = Path.Combine(settingPath, GetFileName());
			File.WriteAllText(filePath, text);
		}
	}
}