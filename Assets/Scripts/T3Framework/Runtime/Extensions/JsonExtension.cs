using Newtonsoft.Json.Linq;

namespace T3Framework.Runtime.Extensions
{
	public static class JsonExtension
	{
		public static void AddIf(this JContainer token, JToken value, bool predicate)
		{
			if (predicate)
			{
				token.Add(value);
			}
		}

		public static bool TryGetValue<TValue>(this JContainer token, string key, out TValue value)
		{
			if (token[key] != null)
			{
				value = token[key].Value<TValue>();
				return true;
			}

			value = default;
			return false;
		}

		/// <summary> Exception safe. </summary>
		public static TValue Get<TValue>(this JContainer token, string key, TValue defaultValue = default)
		{
			try
			{
				var ret = token.TryGetValue(key, out JToken value) ? value.Value<TValue>() : defaultValue;
				return ret;
			}
			catch
			{
				return defaultValue;
			}
		}

		public static void Set<TValue>(this JObject token, TValue value, params string[] keys)
		{
			JObject current = token;
			foreach (var key in keys[..^1])
			{
				if (!current.ContainsKey(key) || current[key] is not JObject)
				{
					current[key] = new JObject();
				}

				current = (JObject)current[key];
			}

			current[keys[^1]] = JToken.FromObject(value);
		}
	}
}