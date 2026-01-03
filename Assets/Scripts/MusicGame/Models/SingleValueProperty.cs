#nullable enable

using Newtonsoft.Json.Linq;

namespace MusicGame.Models
{
	public abstract class SingleValueProperty<T> : IChartSerializable
	{
		public T? Value { get; set; }

		protected abstract T FromJToken(JToken? token);

		protected abstract JToken ToJToken(T? value);

		public JObject GetSerializationToken() => new() { ["value"] = ToJToken(Value) };

		public static TProperty Deserialize<TProperty>(JObject dict) where TProperty : SingleValueProperty<T>, new()
		{
			TProperty result = new();
			result.Value = result.FromJToken(dict["value"]);
			return result;
		}
	}
}