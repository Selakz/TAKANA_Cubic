using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MusicGame.Components;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime.Extensions;
using UnityEngine;

public class LegacyLayerInfo
{
	// Serializable and Public
	public List<string> LayerNames { get; set; } = new();
	public List<int> TrackBelongings { get; set; } = new();

	// Private

	// Static

	// Defined Functions
	public LegacyLayerInfo()
	{
	}

	public LegacyLayerInfo(List<string> layerNames, List<int> trackBelongings)
	{
		LayerNames = new(layerNames);
		TrackBelongings = new(trackBelongings);
	}

	public static LegacyLayerInfo Load(string path)
	{
		return JsonConvert.DeserializeObject<LegacyLayerInfo>(File.ReadAllText(path));
	}

	public void Save(string path)
	{
		File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
	}

	/// <summary>
	/// 返回完整的带换行的双行图层语句
	/// </summary>
	public override string ToString()
	{
		StringBuilder sb = new();
		if (LayerNames.Count > 0)
		{
			sb.Append("layername(");
			for (int i = 0; i < LayerNames.Count - 1; i++)
			{
				sb.Append($"{i + 3}, {LayerNames[i]}, ");
			}

			sb.AppendLine($"{LayerNames.Count + 2}, {LayerNames[^1]});");
		}

		if (TrackBelongings.Count > 0)
		{
			sb.Append("layerorder(");
			for (int i = 0; i < TrackBelongings.Count - 1; i++)
			{
				sb.Append($"{TrackBelongings[i]}, ");
			}

			sb.AppendLine($"{TrackBelongings[^1]});");
		}

		return sb.ToString();
	}
}

public class LayerInfo : ISerializable
{
	public static string TypeMark => "layer";

	public int Id { get; set; }

	public string Name { get; set; }

	public Color Color { get; set; }

	public bool IsDecoration { get; set; }

	public bool IsSelected { get; set; }

	public JToken GetSerializationToken()
	{
		return new JObject
		{
			["id"] = Id,
			["name"] = Name,
			["color"] = Color.ToHexAlphaTuple(),
			["isDecoration"] = IsDecoration,
			["isSelected"] = IsSelected
		};
	}

	public static LayerInfo Deserialize(JToken token)
	{
		if (token is not JContainer container) return default;
		return new()
		{
			Id = container.TryGetValue("id", out int id) ? id : 0,
			Name = container.TryGetValue("name", out string layerName) ? layerName : "Unnamed Layer",
			Color = container.TryGetValue("color", out string hexAlphaTuple)
				? UnityParser.ParseHexAlphaTuple(hexAlphaTuple)
				: Color.black,
			IsDecoration = container.TryGetValue("isDecoration", out bool isDecoration) && isDecoration,
			IsSelected = !container.TryGetValue("isSelected", out bool isSelected) || isSelected,
		};
	}
}