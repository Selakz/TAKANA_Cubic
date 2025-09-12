using System;
using Newtonsoft.Json;

namespace T3Framework.Runtime.Serialization.Json
{
	public class T3TimeConverter : JsonConverter<T3Time>
	{
		public override T3Time ReadJson(JsonReader reader, Type objectType, T3Time existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Integer)
			{
				int value = Convert.ToInt32(reader.Value);
				return new T3Time(value);
			}

			throw new JsonSerializationException($"Expected integer, got {reader.TokenType}");
		}

		public override void WriteJson(JsonWriter writer, T3Time value, JsonSerializer serializer)
		{
			writer.WriteValue(value.Milli);
		}
	}
}