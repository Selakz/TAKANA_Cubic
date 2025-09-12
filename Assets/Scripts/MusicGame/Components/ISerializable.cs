using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MusicGame.Components
{
	/// <summary>
	/// Subclass of <see cref="ISerializable"/> should implement Deserialize method like one of these:
	/// <code>
	/// public static T Deserialize(JToken token, object context);
	/// </code>
	/// <code>
	/// public static T Deserialize(JToken token);
	/// </code>
	/// <br/>Also, it should implement a string property named "TypeMark" whatever static or not.
	/// </summary>
	public interface ISerializable
	{
		private static readonly Dictionary<string, Type> typeDict = new();

		public JToken GetSerializationToken();

		public JToken Serialize(bool addType)
		{
			var token = GetSerializationToken();
			if (token is JContainer && addType)
			{
				var type = GetType();
				token["type"] = type.FullName;
				var typeMarks = typeDict.Where(pair => pair.Value == type).ToList();
				if (typeMarks.Any()) token["type"] = typeMarks.First().Key;
				// non-static TypeMark check
				else
				{
					var property = type.GetProperty(
						"TypeMark",
						BindingFlags.Public | BindingFlags.Instance
					);
					if (property != null && property.PropertyType == typeof(string))
					{
						token["type"] = (string)property.GetValue(this);
					}
				}
			}

			return token;
		}

		/// <summary>
		/// Try to invoke T.Deserialize(JToken, object) where T is token["type"]
		/// </summary>
		/// <param name="json">The serialization of token identifying T</param>
		/// <param name="context">The context for helping serialization</param>
		public static object Deserialize(string json, object context)
		{
			var token = JToken.Parse(json);
			return Deserialize(token, context);
		}

		/// <summary>
		/// Try to invoke T.Deserialize(JToken, object) where T is token["type"]
		/// </summary>
		/// <param name="token">The token identifying T</param>
		/// <param name="context">The context for helping serialization</param>
		public static object Deserialize(JToken token, object context = null)
		{
			var typeMark = token["type"].Value<string>();
			var type = typeDict.TryGetValue(typeMark, out Type t) ? t : Type.GetType(typeMark);
			if (type == null)
			{
				throw new InvalidCastException($"Cannot find type {token["type"]}");
			}

			if (context != null)
			{
				var methodWithContext = type.GetMethod(
					"Deserialize",
					BindingFlags.Public | BindingFlags.Static,
					null,
					new[] { typeof(JToken), typeof(object) },
					null
				);
				if (methodWithContext != null && methodWithContext.ReturnType == type)
				{
					return methodWithContext.Invoke(null, new[] { token, context });
				}
			}

			var method = type.GetMethod(
				"Deserialize",
				BindingFlags.Public | BindingFlags.Static,
				null,
				new[] { typeof(JToken) },
				null
			);
			if (method != null && method.ReturnType == type)
			{
				return method.Invoke(null, new object[] { token });
			}

			throw new InvalidOperationException("Cannot find deserialize method in " + type.FullName);
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		internal static void Initialize()
		{
			var types = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(t => t.IsClass && !t.IsAbstract && typeof(ISerializable).IsAssignableFrom(t));

			foreach (var type in types)
			{
				var property = type.GetProperty(
					"TypeMark",
					BindingFlags.Public | BindingFlags.Static
				);
				if (property != null && property.PropertyType == typeof(string))
				{
					string mark = (string)property.GetValue(null);
					typeDict.Add(mark, type);
				}
			}
		}
	}
}