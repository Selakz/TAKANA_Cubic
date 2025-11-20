using System;
using System.IO;
using System.Reflection;
using T3Framework.Static;
using UnityEngine;

namespace T3Framework.Runtime.Setting
{
	public static class SingletonSettingRegistrar
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void SettingFactory()
		{
			SingletonFactories.Register("PlainTextSetting", type =>
			{
				var settingPath = Path.Combine(Application.persistentDataPath, "Settings");
				if (!Directory.Exists(settingPath)) Directory.CreateDirectory(settingPath);

				var fileNameAttribute = type.GetCustomAttribute<SettingFileNameAttribute>(true);
				var fileName = fileNameAttribute is null ? $"{type.Name}.yaml" : fileNameAttribute.FileName;
				var filePath = Path.Combine(settingPath, fileName);
				if (File.Exists(filePath))
				{
					var yaml = File.ReadAllText(filePath);
					return SettingConvertHelper.Deserializer.Deserialize(yaml, type);
				}

				var instance = Activator.CreateInstance(type);
				var text = SettingConvertHelper.Serializer.Serialize(instance);
				File.WriteAllText(filePath, text);
				return instance;
			});
		}
	}

	/// <summary> If the instance does not exist, this method will create a default file on it. </summary>
	[SingletonFactory("PlainTextSetting")]
	public interface ISingletonSetting<out T> : ISetting<T>, ISingleton<T> where T : ISingletonSetting<T>, new()
	{
		// public static T FromFile()
		// {
		// 	var settingPath = Path.Combine(Application.persistentDataPath, "Settings");
		// 	if (!Directory.Exists(settingPath)) Directory.CreateDirectory(settingPath);
		// 	var filePath = Path.Combine(settingPath, GetFileName());
		// 	if (File.Exists(filePath))
		// 	{
		// 		var yaml = File.ReadAllText(filePath);
		// 		return SettingConvertHelper.Deserializer.Deserialize<T>(yaml);
		// 	}
		//
		// 	var instance = new T();
		// 	var text = SettingConvertHelper.Serializer.Serialize(instance);
		// 	File.WriteAllText(filePath, text);
		// 	return instance;
		// }

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