#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MusicGame.Models
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class ChartTypeMarkAttribute : Attribute
	{
		public string TypeMark { get; }

		public ChartTypeMarkAttribute(string typeMark) => TypeMark = typeMark;
	}

	/// <summary>
	/// For serialize objects with type and customized format.
	/// Implementation of <see cref="IChartSerializable"/> should implement Deserialize method like one of these:
	/// <code>
	/// public static T Deserialize(JObject token, object context);
	/// </code>
	/// <code>
	/// public static T Deserialize(JObject token);
	/// </code>
	/// <br/>Also, it should have a <see cref="ChartTypeMarkAttribute"/> on it.
	/// </summary>
	public interface IChartSerializable
	{
		private static readonly Dictionary<string, Type> typeDict = new();

		public static IChartSerializable Clone(IChartSerializable obj, object? context = null)
		{
			var serialized = obj.Serialize(true);
			return context is null
				? (IChartSerializable)Deserialize(serialized)
				: (IChartSerializable)Deserialize(serialized, context);
		}

		public JObject GetSerializationToken();

		public JObject Serialize(bool addType)
		{
			var dict = GetSerializationToken();
			if (addType)
			{
				var type = GetType();
				dict["type"] = type.FullName;
				var typeMarks = typeDict.Where(pair => pair.Value == type).ToList();
				if (typeMarks.Any()) dict["type"] = typeMarks.First().Key;
			}

			return dict;
		}

		public static object Deserialize(Type type, JObject dict, object? context = null)
		{
			if (!typeof(IChartSerializable).IsAssignableFrom(type))
				throw new InvalidCastException($"Cannot deserialize type {type}");

			if (context != null)
			{
				var methodWithContext = type.GetMethod(
					"Deserialize",
					BindingFlags.Public | BindingFlags.Static,
					null,
					new[] { typeof(JObject), typeof(object) },
					null
				);
				if (methodWithContext != null && methodWithContext.ReturnType == type)
				{
					return methodWithContext.Invoke(null, new[] { dict, context });
				}
			}

			var method = type.GetMethod(
				"Deserialize",
				BindingFlags.Public | BindingFlags.Static,
				null,
				new[] { typeof(JObject) },
				null
			);
			if (method != null && method.ReturnType == type)
			{
				return method.Invoke(null, new object[] { dict });
			}

			throw new InvalidOperationException("Cannot find deserialize method in " + type.FullName);
		}

		/// <summary>
		/// Try to invoke T.Deserialize(JObject, object) where T is token["type"]
		/// </summary>
		/// <param name="dict">The token identifying T</param>
		/// <param name="context">The context for helping serialization</param>
		public static object Deserialize(JObject dict, object? context = null)
		{
			var typeMark = dict["type"]!.Value<string>();
			var type = typeDict.TryGetValue(typeMark!, out Type t) ? t : Type.GetType(typeMark!);
			if (type == null) throw new InvalidCastException($"Cannot find type {dict["type"]}");

			return Deserialize(type, dict, context);
		}

		public static T? Deserialize<T>(JObject dict, object? context = null) where T : IChartSerializable
		{
			try
			{
				return (T?)Deserialize(typeof(T), dict, context);
			}
			catch
			{
				return default;
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		internal static void Initialize()
		{
			var types = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(t => t.IsClass && !t.IsAbstract && typeof(IChartSerializable).IsAssignableFrom(t));

			foreach (var type in types)
			{
				var attribute = type.GetCustomAttribute<ChartTypeMarkAttribute>(false);
				string mark = attribute is null ? type.FullName! : attribute.TypeMark;
				typeDict.Add(mark, type);
			}
		}
	}
}