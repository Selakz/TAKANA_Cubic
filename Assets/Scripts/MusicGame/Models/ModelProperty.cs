#nullable enable

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime.Extensions;

namespace MusicGame.Models
{
	[ChartTypeMark("property")]
	public class ModelProperty : IChartSerializable
	{
		private readonly Dictionary<string, IChartSerializable> properties = new();

		public IChartSerializable? this[string id]
		{
			get => properties.GetValueOrDefault(id);
			set => properties.AddIf(id, value!, !properties.ContainsKey(id) && value != null);
		}

		public int Count => properties.Count;

		/// <summary>
		/// Generally do not call it directly but encapsulate it in extension methods in different modules.
		/// </summary>
		public T? Get<T>(string key) where T : IChartSerializable
		{
			if (properties.TryGetValue(key, out var property) && property is T target) return target;
			return default;
		}

		public bool Remove(string key) => properties.Remove(key);

		public void Clear() => properties.Clear();

		public JObject GetSerializationToken()
		{
			JObject dict = new();
			foreach (var pair in properties) dict.Add(pair.Key, pair.Value.Serialize(true));
			return dict;
		}

		public static ModelProperty Deserialize(JObject dict)
		{
			ModelProperty modelProperty = new();
			foreach (var pair in dict)
			{
				if (pair.Value is not JObject propDict) continue;
				modelProperty[pair.Key] = (IChartSerializable)IChartSerializable.Deserialize(propDict);
			}

			return modelProperty;
		}
	}
}