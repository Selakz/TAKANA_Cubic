#nullable enable

using System;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace T3Framework.Static.Event
{
	public class NotifiablePropertySerializer : IYamlTypeConverter
	{
		public bool Accepts(Type type)
		{
			if (type.IsGenericType)
			{
				var genericTypeDefinition = type.GetGenericTypeDefinition();
				return genericTypeDefinition == typeof(NotifiableProperty<>) ||
				       typeof(NotifiableProperty<>).IsAssignableFrom(genericTypeDefinition);
			}

			return false;
		}

		public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
		{
			var valueType = type.GetGenericArguments()[0];
			var innerValue = rootDeserializer.Invoke(valueType);

			var constructorArgs = new[] { innerValue };
			var constructor = type.GetConstructor(new[] { valueType });
			if (constructor == null)
				throw new InvalidOperationException(
					$"Type {type.Name} does not have a constructor with a single parameter of type {valueType.Name}.");

			var instance = constructor.Invoke(constructorArgs);
			return instance;
		}

		public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
		{
			if (value == null) return;

			var valueProperty = type.GetProperty("Value");
			if (valueProperty == null)
			{
				throw new InvalidOperationException($"Type {type.Name} does not have a 'Value' property.");
			}

			var innerValue = valueProperty.GetValue(value);
			serializer.Invoke(innerValue);
		}
	}
}