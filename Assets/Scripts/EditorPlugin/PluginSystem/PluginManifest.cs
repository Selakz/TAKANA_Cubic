#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using T3Framework.Runtime.I18N;

namespace EditorPlugin.PluginSystem
{
	public interface IParamMeta
	{
	}

	public class PluginParam
	{
		public string Id { get; }

		public I18NString Name { get; }

		public Type Type { get; }

		public string? Meta { get; }

		[JsonIgnore]
		public IReadOnlyCollection<IParamMeta> Metas { get; }

		public PluginParam(string id, I18NString name, Type type, string? meta = null)
		{
			Id = id;
			Name = name;
			Type = type;
			Meta = meta;
			Metas = ParamMetaParser.Parse(meta, type);
		}

		public static object GetDefaultValue(Type type)
		{
			return type switch
			{
				not null when type == typeof(string) => string.Empty,
				not null when type == typeof(float) => 0f,
				not null when type == typeof(int) => 0,
				not null when type == typeof(bool) => false,
				_ => throw new ArgumentException($"Unsupported plugin param type: {type}", nameof(type))
			};
		}
	}

	public class HotkeyPreset
	{
		private readonly Dictionary<string, object> paramValues = new();

		public IReadOnlyDictionary<string, object> Params => paramValues;

		public int Hotkey { get; set; } = 0;

		public void SetParam(string id, object value)
		{
			if (paramValues.ContainsKey(id)) paramValues[id] = value;
		}

		private HotkeyPreset()
		{
		}

		public static HotkeyPreset FromPlugin(PluginInstance plugin, int hotKeyIndex)
		{
			var preset = new HotkeyPreset { Hotkey = hotKeyIndex };
			foreach (var param in plugin.Manifest.Params)
			{
				preset.paramValues[param.Id] = PluginParam.GetDefaultValue(param.Type);
			}

			return preset;
		}
	}

	public sealed class PluginManifest
	{
		public I18NString Name { get; set; }

		public IReadOnlyList<PluginParam> Params { get; set; }

		[DefaultValue(true)]
		public bool ShouldTick { get; set; }

		[DefaultValue(true)]
		public bool IsLazy { get; set; }

		public I18NString Description { get; set; }

		public string ApiVersion { get; set; }

		public string TsEntry { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
		[DefaultValue("dist/main.js")]
		public string JsEntry { get; set; }

		public List<HotkeyPreset> HotkeyPresets { get; set; } = new();

		public PluginManifest(
			I18NString name, IReadOnlyList<PluginParam> @params, bool shouldTick, bool isLazy,
			I18NString description, string apiVersion, string tsEntry, string? jsEntry = null)
		{
			Name = name;
			Params = @params;
			ShouldTick = shouldTick;
			IsLazy = isLazy;
			Description = description;
			ApiVersion = apiVersion;
			TsEntry = tsEntry;
			JsEntry = jsEntry ?? "dist/main.js";
		}
	}
}