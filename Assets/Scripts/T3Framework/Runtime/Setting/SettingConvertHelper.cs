using T3Framework.Runtime.Serialization.Yaml;
using T3Framework.Static.Event;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace T3Framework.Runtime.Setting
{
	public static class SettingConvertHelper
	{
		public static ISerializer Serializer { get; } = new SerializerBuilder()
			.WithEventEmitter(next => new CommentEventEmitter(next))
			.WithTypeConverter(new T3TimeYamlConverter())
			.WithTypeConverter(new ColorYamlConverter())
			.WithTypeConverter(new I18NStringYamlConverter())
			.WithTypeConverter(new NotifiablePropertySerializer())
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.Build();

		public static IDeserializer Deserializer { get; } = new DeserializerBuilder()
			.WithTypeConverter(new T3TimeYamlConverter())
			.WithTypeConverter(new ColorYamlConverter())
			.WithTypeConverter(new I18NStringYamlConverter())
			.WithTypeConverter(new NotifiablePropertySerializer())
			.WithNamingConvention(CamelCaseNamingConvention.Instance)
			.IgnoreUnmatchedProperties()
			.Build();
	}
}