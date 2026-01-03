#nullable enable

using System.Collections.Generic;
using MusicGame.ChartEditor.TrackLayer;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.JudgeLine;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using T3Framework.Static.Easing;

namespace MusicGame.Utility.JsonV1ToV2
{
	public static class V1ToV2Converter
	{
		// Just enumerate all the existing features in previous version and convert them to new format.
		public static ChartInfo DeserializeFromV1(JToken token)
		{
			ChartInfo chart = new();
			if (token is not JObject dict) return chart;
			chart.Properties = GetChartProperties(dict);
			chart.EditorConfig = GetChartEditorConfig(dict);

			if (dict["components"] is not JArray componentArray) return chart;
			Dictionary<int, ChartComponent> components = new();
			foreach (var componentToken in componentArray)
			{
				if (componentToken is not JObject componentDict) continue;
				var id = componentDict.Get("id", -1);
				var type = componentDict.Get("type", string.Empty);
				if (id == -1 || string.IsNullOrEmpty(type)) continue;
				int parentId = -1;
				IChartModel? model = null;
				switch (type)
				{
					case "judgeLine":
						model = new StaticJudgeLine();
						break;
					case "e_track" or "track":
						model = GetTrack(componentDict);
						parentId = componentDict.Get("line", -1);
						break;
					case "e_tap" or "tap" or "e_slide" or "slide":
						model = GetHit(componentDict, type);
						parentId = componentDict.Get("track", -1);
						break;
					case "e_hold" or "hold":
						model = GetHold(componentDict);
						parentId = componentDict.Get("track", -1);
						break;
					default:
						break;
				}

				if (model is not null)
				{
					var component = chart.AddComponent(model);
					component.Id = id;
					component.Name = GetName(componentDict);
					components[id] = component;
					if (parentId != -1) component.SetParent(components[parentId]);
				}
			}

			return chart;
		}

		private static ModelProperty GetChartProperties(JObject dict)
		{
			ModelProperty properties = new();
			if (dict["properties"] is not JObject propDict) return properties;
			// offset
			OffsetInfo offsetInfo = new(propDict.Get("offset", 0));
			properties["offset"] = offsetInfo;
			return properties;
		}

		private static ModelProperty GetChartEditorConfig(JObject dict)
		{
			ModelProperty editorconfig = new();
			if (dict["properties"] is not JObject propDict ||
			    propDict["editorconfig"] is not JObject editorDict) return editorconfig;
			// layers
			if (editorDict["layers"] is JArray layerArray)
			{
				LayersInfo layersInfo = new();
				foreach (var layerToken in layerArray)
				{
					if (layerToken is not JObject layerDict) continue;
					layersInfo.Add(new LayerComponent(layersInfo, LayerInfo.Deserialize(layerDict)));
				}

				editorconfig["layers"] = layersInfo;
			}

			return editorconfig;
		}

		private static string? GetName(JObject dict)
		{
			return dict["editorconfig"] is JObject editorDict ? editorDict.Get<string>("name") : null;
		}

		private static Track GetTrack(JObject dict)
		{
			T3Time timeStart = dict["timeStart"]!.Value<int>();
			T3Time timeEnd = dict["timeEnd"]!.Value<int>();

			ChartPosMoveList left = new(), right = new();
			if (dict["movement"] is JObject movementDict)
			{
				if (movementDict["left"]?["list"] is JArray leftArray)
				{
					foreach (var token in leftArray)
					{
						var match = RegexHelper.MatchTuple(token.Value<string>(), 3);
						var time = T3Time.Parse(match.Groups[1].Value);
						var position = float.Parse(match.Groups[2].Value);
						var ease = CurveCalculator.GetEaseByName(match.Groups[3].Value.Trim());
						left.Insert(time, new V1EMoveItem(position, ease));
					}
				}

				if (movementDict["right"]?["list"] is JArray rightArray)
				{
					foreach (var token in rightArray)
					{
						var match = RegexHelper.MatchTuple(token.Value<string>(), 3);
						var time = T3Time.Parse(match.Groups[1].Value);
						var position = float.Parse(match.Groups[2].Value);
						var ease = CurveCalculator.GetEaseByName(match.Groups[3].Value.Trim());
						right.Insert(time, new V1EMoveItem(position, ease));
					}
				}
			}

			var track = new Track(timeStart, timeEnd) { Movement = new TrackEdgeMovement(left, right) };
			track.SetProperties(dict);
			if (dict["editorconfig"] is JObject editorDict && editorDict.TryGetValue("layer", out int layerId))
			{
				LayerReference layerReference = new(layerId);
				track.EditorConfig["layer"] = layerReference;
			}

			return track;
		}

		private static Hit GetHit(JObject dict, string type)
		{
			var hit = Hit.Deserialize(dict);
			hit.Type = type switch
			{
				"e_slide" or "slide" => HitType.Slide,
				_ => HitType.Tap
			};
			return hit;
		}

		private static Hold GetHold(JObject dict)
		{
			var hold = Hold.Deserialize(dict);
			return hold;
		}
	}
}