#nullable enable

using System;
using T3Framework.Runtime.Localization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace T3Framework.Runtime.Serialization.Yaml
{
	public class I18NStringYamlConverter : IYamlTypeConverter
	{
		public bool Accepts(Type type) => type == typeof(I18NString);

		public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
		{
			I18NString result = new();
			parser.Consume<MappingStart>();

#pragma warning disable CS8600
			while (!parser.TryConsume(out MappingEnd _))
#pragma warning restore CS8600
			{
				string abbreviation = parser.Consume<Scalar>().Value;
				string content = parser.Consume<Scalar>().Value;
				var language = LanguageExtension.GetLanguage(abbreviation);
				if (language is null) continue;
				result.Add(language.Value, content);
			}

			return result;
		}

		public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
		{
			emitter.Emit(new MappingStart());

			I18NString str = (I18NString)value!;

			foreach (var pair in str)
			{
				if (string.IsNullOrEmpty(pair.Value)) continue;
				emitter.Emit(new Scalar(pair.Key.GetAbbreviation()));
				emitter.Emit(new Scalar(pair.Value));
			}

			emitter.Emit(new MappingEnd());
		}
	}
}