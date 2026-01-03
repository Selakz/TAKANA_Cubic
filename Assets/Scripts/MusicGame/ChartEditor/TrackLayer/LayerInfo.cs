#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Track;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Extensions;
using T3Framework.Static;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLayer
{
	[ChartTypeMark("trackLayers")]
	public class LayersInfo : RootDataset<LayerComponent>, IChartSerializable
	{
		private readonly List<LayerComponent> layers = new();

		public event Action? OnOrderChanged;

		public int NewId => layers.Max(l => l.Model.Id) + 1;

		public override int Count => layers.Count;

		public LayerInfo? this[int id] => layers.FirstOrDefault(l => l.Model.Id == id)?.Model ?? null;

		public LayerInfo DefaultLayer => this[ISingleton<TrackLayerSetting>.Instance.DefaultLayerId]!;

		/// <summary> Will automatically add a default layer. </summary>
		public LayersInfo() => layers.Add(new LayerComponent(this, new LayerInfo())); // Default LayerInfo

		public void Sort(Comparison<LayerComponent> comparison)
		{
			layers.Sort(comparison);
			OnOrderChanged?.Invoke();
		}

		public override bool Contains(LayerComponent item) => layers.Contains(item);

		/// <returns> The *reverse* index of the layer. </returns>
		public int IndexOf(int id)
		{
			var index = layers.FindIndex(l => l.Model.Id == id);
			if (index is -1) return -1;
			else return Count - index;
		}

		protected override bool AddToDataset(LayerComponent item)
		{
			// This removal theoretically does nothing while editing, and is for deserialization.
			layers.RemoveAll(l => l.Model.Id == item.Model.Id);
			layers.Add(item);
			return true;
		}

		protected override bool IsRemovable(LayerComponent item) =>
			item.Model.Id != ISingleton<TrackLayerSetting>.Instance.DefaultLayerId;

		protected override void RemoveFromDataset(LayerComponent item) => layers.Remove(item);

		protected override bool NeedToRemove(LayerComponent item) => false;

		public override IEnumerator<LayerComponent> GetEnumerator() => layers.GetEnumerator();

		public JObject GetSerializationToken()
		{
			JObject dict = new();
			JArray layerArray = new();
			foreach (var layer in layers) layerArray.Add(layer.Model.GetSerializationToken());
			dict.Add("value", layerArray);
			return dict;
		}

		public static LayersInfo Deserialize(JObject dict)
		{
			LayersInfo layers = new();
			JArray layerArray = dict.Get("value", new JArray());
			foreach (var layerToken in layerArray)
			{
				if (layerToken is not JObject layerDict) continue;
				layers.Add(new LayerComponent(layers, LayerInfo.Deserialize(layerDict)));
			}

			return layers;
		}
	}

	public class LayerComponent : IComponent<LayerInfo>
	{
		public event EventHandler? OnComponentUpdated;
		public void UpdateNotify() => OnComponentUpdated?.Invoke(this, EventArgs.Empty);

		public LayersInfo Parent { get; }

		public LayerInfo Model { get; }

		public LayerComponent(LayersInfo parent, LayerInfo model)
		{
			Parent = parent;
			Model = model;
		}

		public void UpdateModel(Action<LayerInfo> action)
		{
			action.Invoke(Model);
			OnComponentUpdated?.Invoke(this, EventArgs.Empty);
		}
	}

	[ChartTypeMark("trackLayer")]
	public class LayerInfo : IChartSerializable
	{
		public int Id { get; set; } = ISingleton<TrackLayerSetting>.Instance.DefaultLayerId;

		public string Name { get; set; } = ISingleton<TrackLayerSetting>.Instance.DefaultLayerName;

		public Color Color { get; set; } = ISingleton<TrackLayerSetting>.Instance.DefaultColor;

		public bool IsDecoration { get; set; } = false;

		public bool IsSelected { get; set; } = true;

		public JObject GetSerializationToken()
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

		public static LayerInfo Deserialize(JObject dict)
		{
			return new()
			{
				Id = dict.Get("id", 0),
				Name = dict.Get("name", "Unnamed Layer"),
				Color = dict.TryGetValue("color", out string hexAlphaTuple)
					? UnityParser.ParseHexAlphaTuple(hexAlphaTuple)
					: Color.black,
				IsDecoration = dict.TryGetValue("isDecoration", out bool isDecoration) && isDecoration,
				IsSelected = !dict.TryGetValue("isSelected", out bool isSelected) || isSelected,
			};
		}
	}

	[ChartTypeMark("layerRef")]
	public class LayerReference : IChartSerializable
	{
		public int LayerId { get; set; } = 0;

		public LayerReference(int layerId) => LayerId = layerId;

		public JObject GetSerializationToken() => new() { ["id"] = LayerId };

		public static LayerReference Deserialize(JObject dict) => new(dict.Get("id", 0));
	}

	public static class LayerInfoExtensions
	{
		public static int GetLayerId(this ITrack track)
		{
			var reference = track.EditorConfig.Get<LayerReference>("layer");
			return reference?.LayerId ?? ISingleton<TrackLayerSetting>.Instance.DefaultLayerId;
		}

		public static void SetLayer(this ITrack track, int id)
		{
			var reference = track.EditorConfig.Get<LayerReference>("layer");
			if (reference is null)
			{
				reference = new LayerReference(id);
				track.EditorConfig["layer"] = reference;
			}
			else
			{
				reference.LayerId = id;
			}
		}

		public static void SetDefaultLayer(this ITrack track)
		{
			var id = ISingleton<TrackLayerSetting>.Instance.DefaultLayerId;
			SetLayer(track, id);
		}

		/// <summary> "Gets" means if there isn't any, this method will create one. </summary>
		public static LayersInfo GetsLayersInfo(this ChartInfo chart)
		{
			var layersInfo = chart.EditorConfig.Get<LayersInfo>("layers");
			if (layersInfo is null)
			{
				layersInfo = new LayersInfo();
				chart.EditorConfig["layers"] = layersInfo;
			}

			return layersInfo;
		}

		public static LayerInfo? GetLayerInfo(this ChartComponent component)
		{
			if (component is not { Model: ITrack track, BelongingChart: { } chart }) return null;
			var layersInfo = chart.GetsLayersInfo();
			var id = track.GetLayerId();
			return layersInfo[id];
		}
	}
}