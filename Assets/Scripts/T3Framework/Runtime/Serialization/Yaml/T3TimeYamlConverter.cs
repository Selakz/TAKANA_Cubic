using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace T3Framework.Runtime.Serialization.Yaml
{
	public class T3TimeYamlConverter : IYamlTypeConverter
	{
		public bool Accepts(Type type)
		{
			return type == typeof(T3Time);
		}

		public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
		{
			var scalar = parser.Consume<Scalar>();
			string value = scalar.Value;

			return T3Time.Parse(value);
		}

		public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
		{
			var time = (T3Time)value!;
			emitter.Emit(new Scalar(time.ToString()));
		}
	}
}