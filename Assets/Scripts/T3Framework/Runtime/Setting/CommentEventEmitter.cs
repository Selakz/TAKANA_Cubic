using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Serialization.NamingConventions;

namespace T3Framework.Runtime.Setting
{
	public class CommentEventEmitter : ChainedEventEmitter
	{
		private readonly Dictionary<Type, Dictionary<string, string>> commentCache = new();
		private Type currentSerializingType = null;

		public CommentEventEmitter(IEventEmitter nextEmitter) : base(nextEmitter)
		{
		}

		public override void Emit(MappingStartEventInfo eventInfo, IEmitter emitter)
		{
			CacheTypeComments(eventInfo.Source.Type);
			base.Emit(eventInfo, emitter);
		}

		public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
		{
			if (ShouldAddComment(eventInfo, out var comment))
			{
				emitter.Emit(new Comment(comment, false));
			}

			base.Emit(eventInfo, emitter);
		}

		private bool ShouldAddComment(ScalarEventInfo eventInfo, out string comment)
		{
			comment = null;
			if (eventInfo.Source.Value == null)
			{
				return false;
			}

			var propertyName = CamelCaseNamingConvention.Instance.Reverse(eventInfo.Source.Value.ToString());
			return commentCache.TryGetValue(currentSerializingType, out var properties) &&
			       properties.TryGetValue(propertyName, out comment);
		}

		private void CacheTypeComments(Type type)
		{
			currentSerializingType = type;
			if (commentCache.ContainsKey(type)) return;

			var commentMap = new Dictionary<string, string>();
			foreach (var property in type.GetProperties())
			{
				var attr = property.GetCustomAttribute<DescriptionAttribute>();
				if (attr != null)
				{
					commentMap[property.Name] = attr.Description;
				}
			}

			commentCache[type] = commentMap;
		}
	}
}