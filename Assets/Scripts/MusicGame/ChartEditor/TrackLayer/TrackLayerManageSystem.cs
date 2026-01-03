#nullable enable

using System;
using System.Linq;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using T3Framework.Static.Event;

namespace MusicGame.ChartEditor.TrackLayer
{
	public class TrackLayerManageSystem : T3System
	{
		// Serializable and Public
		public NotifiableProperty<LayersInfo?> LayersInfo { get; } = new(null);

		public event Action<ChartComponent>? OnTrackUpdate;

		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfoProperty, (_, _) =>
			{
				var levelInfo = levelInfoProperty.Value;
				if (levelInfo is null)
				{
					LayersInfo.Value = null;
					return;
				}

				if (chart is not null) chart.OnComponentModelUpdated -= OnTrackUpdated;
				chart = levelInfo.Chart;
				chart.OnComponentModelUpdated += OnTrackUpdated;
				LayersInfo.Value = chart.GetsLayersInfo();
			}),
			new PropertyNestedRegistrar<LayersInfo?>(LayersInfo, value => new DatasetRegistrar<LayerComponent>
			(value!, DatasetRegistrar<LayerComponent>.RegisterTarget.DataRemoved, layer =>
			{
				if (chart is not null)
				{
					foreach (var component in chart)
					{
						if (component.Model is ITrack track && track.GetLayerId() == layer.Model.Id)
						{
							component.UpdateModel(model => ((ITrack)model).SetDefaultLayer());
						}
					}
				}
			}))
		};

		// Private
		private readonly NotifiableProperty<LevelInfo?> levelInfoProperty;
		private ChartInfo? chart;

		// Defined Functions
		public TrackLayerManageSystem(
			NotifiableProperty<LevelInfo?> levelInfoProperty) : base(true)
		{
			this.levelInfoProperty = levelInfoProperty;
		}

		public LayerInfo GetLayer(ChartComponent component)
		{
			if (component.Model is not ITrack track) return LayersInfo.Value!.DefaultLayer;
			return LayersInfo.Value![track.GetLayerId()] ?? LayersInfo.Value!.DefaultLayer;
		}

		public bool TrySetLayerDecoration(LayerComponent layer, bool isDecoration)
		{
			if (layer.Model.IsDecoration == isDecoration) return false;
			if (chart is null) return false;
			if (isDecoration)
			{
				foreach (var component in chart)
				{
					if (component.Model is not ITrack track || track.GetLayerId() != layer.Model.Id) continue;
					var children = component.Children;
					if (children.Any(c => c.Model is INote))
					{
						T3Logger.Log("Notice",
							$"TrackLayer_Decoration_SetTrueFailForNote|{component.Id}", T3LogType.Warn);
						return false;
					}
				}

				layer.UpdateModel(model => model.IsDecoration = isDecoration);
				T3Logger.Log("Notice", "TrackLayer_Decoration_SetTrueSuccess", T3LogType.Success);
			}
			else
			{
				layer.UpdateModel(model => model.IsDecoration = isDecoration);
				T3Logger.Log("Notice", "TrackLayer_Decoration_SetFalseSuccess", T3LogType.Success);
			}

			return true;
		}

		public int GetTrackCountOnLayer(LayerComponent layer)
		{
			return chart?.Count(component => component.Model is ITrack track && track.GetLayerId() == layer.Model.Id)
			       ?? 0;
		}

		// Event Handlers
		private void OnTrackUpdated(ChartComponent track)
		{
			if (track.Model is not ITrack) return;
			OnTrackUpdate?.Invoke(track);
		}

		// System Functions
		protected override void OnInactive()
		{
			base.OnInactive();
			if (chart is not null) chart.OnComponentModelUpdated -= OnTrackUpdated;
		}
	}
}