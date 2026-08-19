#nullable enable

using System;
using Newtonsoft.Json;
using T3Framework.Runtime.I18N;

namespace T3Framework.Runtime.Serialization.Json
{
	public class I18NStringJsonConverter : JsonConverter<I18NString>
	{
		public override I18NString ReadJson(JsonReader reader, Type objectType, I18NString? existingValue,
			bool hasExistingValue, JsonSerializer serializer)
		{
			I18NString result = existingValue ?? new I18NString();

			if (reader.TokenType != JsonToken.StartObject)
			{
				throw new JsonSerializationException($"Expected start object, got {reader.TokenType}");
			}

			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.EndObject) break;
				if (reader.TokenType != JsonToken.PropertyName)
				{
					throw new JsonSerializationException($"Expected property name, got {reader.TokenType}");
				}

				string abbreviation = (string)reader.Value!;
				string content = reader.ReadAsString() ?? string.Empty;
				var language = LanguageExtension.GetLanguage(abbreviation);
				if (language is null) continue;
				result.Add(language.Value, content);
			}

			return result;
		}

		public override void WriteJson(JsonWriter writer, I18NString? value, JsonSerializer serializer)
		{
			writer.WriteStartObject();

			I18NString str = value!;
			foreach (var pair in str)
			{
				if (string.IsNullOrEmpty(pair.Value)) continue;
				writer.WritePropertyName(pair.Key.GetAbbreviation());
				writer.WriteValue(pair.Value);
			}

			writer.WriteEndObject();
		}
	}
}
