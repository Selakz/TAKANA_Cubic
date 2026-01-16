using System.IO;
using System.Threading.Tasks;

namespace T3Framework.Runtime.Setting
{
	public interface ISetting<out T> where T : ISetting<T>, new()
	{
		/// <summary> If the path does not exist, this method will create a default file on it. </summary>
		public static T Load(string filePath)
		{
			if (File.Exists(filePath))
			{
				var yaml = File.ReadAllText(filePath);
				return SettingConvertHelper.Deserializer.Deserialize<T>(yaml);
			}

			var setting = new T();
			var text = SettingConvertHelper.Serializer.Serialize(setting);
			File.WriteAllText(filePath, text);
			return setting;
		}

		public static async Task<T> LoadAsync(string filePath)
		{
			if (File.Exists(filePath))
			{
				var yaml = await File.ReadAllTextAsync(filePath);
				return SettingConvertHelper.Deserializer.Deserialize<T>(yaml);
			}

			var setting = new T();
			var text = SettingConvertHelper.Serializer.Serialize(setting);
			await File.WriteAllTextAsync(filePath, text);
			return setting;
		}

		public static void Save(T setting, string filePath)
		{
			var text = SettingConvertHelper.Serializer.Serialize(setting);
			File.WriteAllText(filePath, text);
		}

		public static async Task SaveAsync(T setting, string filePath)
		{
			var text = SettingConvertHelper.Serializer.Serialize(setting);
			await File.WriteAllTextAsync(filePath, text);
		}

		public static string GetFileName()
		{
			var fileNameAttributes = typeof(T).GetCustomAttributes(typeof(SettingFileNameAttribute), true);
			if (fileNameAttributes.Length > 0)
			{
				var fileNameAttribute = (SettingFileNameAttribute)fileNameAttributes[0];
				return fileNameAttribute.FileName;
			}
			else
			{
				return $"{typeof(T).Name}.yaml";
			}
		}
	}
}