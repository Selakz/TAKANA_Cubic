using System;
using T3Framework.Runtime.Extensions;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace T3Framework.Runtime.Serialization.Yaml
{
	public class ColorYamlConverter : IYamlTypeConverter
	{
		public bool Accepts(Type type)
		{
			return type == typeof(Color) || type == typeof(Color?);
		}

		public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
		{
			var scalar = parser.Consume<Scalar>();
			string value = scalar.Value;
			return UnityParser.ParseHexAlphaTuple(value);
		}

		public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
		{
			var color = (Color)value!;
			string str = color.ToHexAlphaTuple();
			emitter.Emit(new Scalar(str));
		}
	}
}