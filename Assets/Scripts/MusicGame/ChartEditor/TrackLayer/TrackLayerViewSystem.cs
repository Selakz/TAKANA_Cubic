#nullable enable

using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Basic;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLayer
{
	public class TrackLayerViewSystem : HierarchySystem<TrackLayerViewSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority layerColorPriority = default!;
		[SerializeField] private SequencePriority layerSortingOrderPriority = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(viewPool, handler => new CustomRegistrar(
				() =>
				{
					var track = viewPool[handler]!;
					if (track.Model is not ITrack) return;
					RegisterTrackView(track);
				},
				() =>
				{
					var track = viewPool[handler]!;
					if (track.Model is not ITrack) return;
					UnregisterTrackView(track);
				})),
			new PropertyRegistrar<LayersInfo?>(manageSystem.LayersInfo, (_, _) =>
			{
				var lastInfo = manageSystem.LayersInfo.LastValue;
				if (lastInfo is not null)
				{
					lastInfo.OnDataUpdated -= OnLayerUpdate;
					lastInfo.OnOrderChanged -= UpdateTrackView;
				}

				var nowInfo = manageSystem.LayersInfo.Value;
				if (nowInfo is not null)
				{
					nowInfo.OnDataUpdated += OnLayerUpdate;
					nowInfo.OnOrderChanged += UpdateTrackView;
				}

				ResetLayerView();
			}),
			new PropertyRegistrar<float>(ISingleton<TrackLayerSetting>.Instance.SelectLayerOpacityRatio,
				UpdateTrackView),
			new PropertyRegistrar<float>(ISingleton<TrackLayerSetting>.Instance.UnselectLayerOpacityRatio,
				UpdateTrackView),
			new CustomRegistrar(
				() => manageSystem.OnTrackUpdate += OnTrackUpdate, () => manageSystem.OnTrackUpdate -= OnTrackUpdate)
		};

		// Private
		[Inject] private TrackLayerManageSystem manageSystem = default!;
		[Inject, Key("stage")] private IViewPool<ChartComponent> viewPool = default!;

		private void OnLayerUpdate(LayerComponent layer)
		{
			foreach (var track in viewPool)
			{
				if (track.Model is not ITrack) continue;
				var layerInfo = manageSystem.GetLayer(track);
				if (layerInfo.Id != layer.Model.Id) continue;
				RegisterTrackView(track);
			}
		}

		private void ResetLayerView()
		{
			foreach (var track in viewPool)
			{
				if (track.Model is not ITrack) continue;
				UnregisterTrackView(track);
			}
		}

		private void OnTrackUpdate(ChartComponent track)
		{
			if (viewPool.Contains(track)) RegisterTrackView(track);
		}

		private void RegisterTrackView(ChartComponent track)
		{
			var layerInfo = manageSystem.GetLayer(track);
			var handler = viewPool[track]!;
			var presenter = handler.Script<IT3ModelViewPresenter>();
			presenter.ColorModifier.Register(
				color =>
				{
					float alphaRate = layerInfo.Color.a * LayerAlphaRate();
					return new Color(layerInfo.Color.r, layerInfo.Color.g, layerInfo.Color.b, color.a * alphaRate);
				},
				layerColorPriority, true);
			// TODO: Rearrange it and select sorting order after fix IT3ModelViewPresenter
			presenter.Textures["main"].SortingOrderModifier.Register(value =>
			{
				var index = track.BelongingChart?.GetsLayersInfo().IndexOf((track.Model as ITrack)!.GetLayerId());
				return index is null or -1 ? value : value + index.Value;
			}, layerSortingOrderPriority);
			presenter.Textures["leftLine"].ColorModifier.Register(
				color =>
				{
					float alphaRate = color.a * LayerAlphaRate();
					return color with { a = alphaRate };
				},
				layerColorPriority);
			presenter.Textures["rightLine"].ColorModifier.Register(
				color =>
				{
					float alphaRate = color.a * LayerAlphaRate();
					return color with { a = alphaRate };
				},
				layerColorPriority);
			if (handler["select-collider"] is { } colliderHandler)
			{
				var colliderPlugin = colliderHandler.Script<SelectColliderPlugin>();
				colliderPlugin.ColliderModifier.EnabledModifier.Register(_ => layerInfo.IsSelected, layerColorPriority);
			}

			return;

			float LayerAlphaRate() => layerInfo.IsSelected
				? ISingleton<TrackLayerSetting>.Instance.SelectLayerOpacityRatio
				: ISingleton<TrackLayerSetting>.Instance.UnselectLayerOpacityRatio;
		}

		private void UnregisterTrackView(ChartComponent track)
		{
			var handler = viewPool[track]!;
			var presenter = handler.Script<IT3ModelViewPresenter>();
			presenter.ColorModifier.Unregister(layerColorPriority, true);
			presenter.Textures["main"].SortingOrderModifier.Unregister(layerSortingOrderPriority);
			presenter.Textures["leftLine"].ColorModifier.Unregister(layerColorPriority);
			presenter.Textures["rightLine"].ColorModifier.Unregister(layerColorPriority);
			if (handler["select-collider"] is { } colliderHandler)
			{
				var colliderPlugin = colliderHandler.Script<SelectColliderPlugin>();
				colliderPlugin.ColliderModifier.EnabledModifier.Unregister(layerColorPriority);
			}
		}

		private void UpdateTrackView()
		{
			foreach (var track in viewPool)
			{
				if (track.Model is not ITrack) continue;
				var handler = viewPool[track]!;
				var presenter = handler.Script<IT3ModelViewPresenter>();
				presenter.Textures["leftLine"].ColorModifier.Update();
				presenter.Textures["rightLine"].ColorModifier.Update();
				presenter.Textures["main"].SortingOrderModifier.Update();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			var info = manageSystem.LayersInfo.Value;
			if (info is not null) info.OnDataUpdated -= OnLayerUpdate;
			ResetLayerView();
		}
	}
}