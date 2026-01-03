#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MusicGame.Models
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class StringTypeMarkAttribute : Attribute
	{
		public string TypeMark { get; }

		public StringTypeMarkAttribute(string typeMark) => TypeMark = typeMark;
	}

	/// <summary>
	/// Similar to <see cref="IChartSerializable"/>, but is serialized to a string value.
	/// The format with type is "[type]_[serialized value]".
	/// <code>
	/// public static T Deserialize(string content);
	/// </code>
	/// <br/>Also, it should have a <see cref="StringTypeMarkAttribute"/> on it.
	/// </summary>
	public interface IStringSerializable
	{
		private static readonly Dictionary<string, Type> typeDict = new();

		public static IStringSerializable Clone(IStringSerializable obj)
		{
			var serialized = obj.Serialize(true);
			return (IStringSerializable)Deserialize(serialized);
		}

		public string Serialize();

		public string Serialize(bool addType)
		{
			var value = Serialize();
			if (addType)
			{
				var type = GetType();
				var mark = type.FullName;
				var typeMarks = typeDict.Where(pair => pair.Value == type).ToList();
				if (typeMarks.Any()) mark = typeMarks.First().Key;
				return $"{mark}_{value}";
			}

			return value;
		}

		public static object Deserialize(Type type, string value)
		{
			if (!typeof(IStringSerializable).IsAssignableFrom(type))
				throw new InvalidCastException($"Cannot deserialize type {type}");

			var split = value.IndexOf('_');
			if (split >= 0) value = value[(split + 1)..];

			var method = type.GetMethod(
				"Deserialize",
				BindingFlags.Public | BindingFlags.Static,
				null,
				new[] { typeof(string) },
				null
			);
			if (method != null && method.ReturnType == type)
			{
				return method.Invoke(null, new object[] { value });
			}

			throw new InvalidOperationException("Cannot find deserialize method in " + type.FullName);
		}

		/// <summary>
		/// Try to invoke T.Deserialize(JObject, object) where T is token["type"]
		/// </summary>
		/// <param name="value"> The token identifying T </param>
		public static object Deserialize(string value)
		{
			var split = value.IndexOf('_');
			if (split < 0) throw new InvalidCastException($"Cannot find type in {value}");
			var typeMark = value[..split];
			var content = value[(split + 1)..];
			var type = typeDict.TryGetValue(typeMark, out Type t) ? t : Type.GetType(typeMark);
			if (type == null) throw new InvalidCastException($"Cannot find type {typeMark}");

			return Deserialize(type, content);
		}

		public static T? Deserialize<T>(string value) where T : IStringSerializable
		{
			try
			{
				return (T?)Deserialize(typeof(T), value);
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
				.Where(t => (t.IsClass || t.IsValueType) && !t.IsAbstract &&
				            typeof(IStringSerializable).IsAssignableFrom(t));

			foreach (var type in types)
			{
				var attribute = type.GetCustomAttribute<StringTypeMarkAttribute>(false);
				string mark = attribute is null ? type.FullName! : attribute.TypeMark;
				typeDict.Add(mark, type);
			}
		}
	}
}