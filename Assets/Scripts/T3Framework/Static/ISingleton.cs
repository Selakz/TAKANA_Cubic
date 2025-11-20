#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;

namespace T3Framework.Static
{
	public static class SingletonFactories
	{
		private static readonly Dictionary<string, Func<Type, object>> factories = new();

		public static void Register(string key, Func<Type, object> factory) => factories[key] = factory;

		public static Func<Type, object> GetFactory(string key) => factories[key]!;
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
	public class SingletonFactoryAttribute : Attribute
	{
		public string Key { get; }

		public SingletonFactoryAttribute(string factoryKey)
		{
			Key = factoryKey;
		}
	}

	public interface ISingleton<out T> where T : ISingleton<T>
	{
		private static T? instance;

		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					var type = typeof(T);
					var attribute = type.GetCustomAttribute<SingletonFactoryAttribute>(true);
					if (attribute is null)
					{
						var interfaces = type.GetInterfaces();
						foreach (var interfaceType in interfaces)
						{
							attribute = interfaceType.GetCustomAttribute<SingletonFactoryAttribute>(true);
							if (attribute is not null) break;
						}

						if (attribute is null)
						{
							throw new InvalidOperationException(
								$"Type {type.FullName} does not have a SingletonFactoryAttribute.");
						}
					}

					var factory = SingletonFactories.GetFactory(attribute.Key);
					var created = factory.Invoke(type);
					if (created is not T factoryInstance)
					{
						throw new InvalidOperationException($"Type {type.FullName} has an error factory method.");
					}

					instance = factoryInstance;
				}

				return instance;
			}
			protected set => instance = value;
		}
	}
}