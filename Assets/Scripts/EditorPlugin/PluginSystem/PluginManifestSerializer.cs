#nullable enable

using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using T3Framework.Runtime.Serialization.Json;

namespace EditorPlugin.PluginSystem
{
	/// <summary> Shared JSON settings used to read and write manifest.json. </summary>
	public static class PluginManifestSerializer
	{
		public static JsonSerializerSettings Settings { get; } = new()
		{
			Converters = { new I18NStringJsonConverter(), new PluginParamTypeJsonConverter() },
			ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
		};

		public static PluginManifest? Load(string manifestPath) =>
			JsonConvert.DeserializeObject<PluginManifest>(File.ReadAllText(manifestPath), Settings);

		public static void Save(string manifestPath, PluginManifest manifest) =>
			File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest, Settings));
	}
}
