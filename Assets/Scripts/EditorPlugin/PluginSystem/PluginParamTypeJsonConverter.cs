#nullable enable

using System;
using Newtonsoft.Json;

namespace EditorPlugin.PluginSystem
{
	public class PluginParamTypeJsonConverter : JsonConverter<Type>
	{
		public override Type ReadJson(JsonReader reader, Type objectType, Type? existingValue,
			bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.String)
			{
				throw new JsonSerializationException($"Expected string, got {reader.TokenType}");
			}

			string typeName = (string)reader.Value!;
			return typeName switch
			{
				"string" => typeof(string),
				"int" => typeof(int),
				"float" => typeof(float),
				"bool" => typeof(bool),
				_ => throw new JsonSerializationException($"Invalid plugin param type: {typeName}")
			};
		}

		public override void WriteJson(JsonWriter writer, Type? value, JsonSerializer serializer)
		{
			string typeName = value switch
			{
				not null when value == typeof(string) => "string",
				not null when value == typeof(int) => "int",
				not null when value == typeof(float) => "float",
				not null when value == typeof(bool) => "bool",
				_ => throw new JsonSerializationException($"Unsupported plugin param type: {value}")
			};

			writer.WriteValue(typeName);
		}
	}
}