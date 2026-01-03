#nullable enable

using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Basic;
using MusicGame.Gameplay.Basic.T3;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Static;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLayer
{
	public class TrackLayerViewSystem : T3System
	{
		private readonly int layerColorPriority;
		private readonly int layerSortingOrderPriority;
		private readonly TrackLayerManageSystem manageSystem;
		private readonly IViewPool<ChartComponent> trackPool;

		public TrackLayerViewSystem(
			int layerColorPriority,
			int layerSortingOrderPriority,
			TrackLayerManageSystem manageSystem,
			T3TrackViewSystem trackViewSystem) : base(true)
		{
			this.layerColorPriority = layerColorPriority;
			this.layerSortingOrderPriority = layerSortingOrderPriority;
			this.manageSystem = manageSystem;
			trackPool = trackViewSystem.TrackPool;
		}

		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
		{
			new CustomRegistrar(
				() => trackPool.OnDataAdded += OnDataAdded,
				() => trackPool.OnDataAdded -= OnDataAdded),
			new CustomRegistrar(
				() => trackPool.BeforeDataRemoved += BeforeDataRemoved,
				() => trackPool.BeforeDataRemoved -= BeforeDataRemoved),
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

		private void OnDataAdded(ChartComponent track) => RegisterTrackView(track);

		private void BeforeDataRemoved(ChartComponent track) => UnregisterTrackView(track);

		private void OnLayerUpdate(LayerComponent layer)
		{
			foreach (var track in trackPool)
			{
				var layerInfo = manageSystem.GetLayer(track);
				if (layerInfo.Id != layer.Model.Id) continue;
				RegisterTrackView(track);
			}
		}

		private void ResetLayerView()
		{
			foreach (var track in trackPool)
			{
				UnregisterTrackView(track);
			}
		}

		private void OnTrackUpdate(ChartComponent track) => RegisterTrackView(track);

		private void RegisterTrackView(ChartComponent track)
		{
			if (!trackPool.Contains(track)) return;
			var layerInfo = manageSystem.GetLayer(track);
			var handler = trackPool[track]!;
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
			if (!trackPool.Contains(track)) return;
			var handler = trackPool[track]!;
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
			foreach (var track in trackPool)
			{
				var handler = trackPool[track]!;
				var presenter = handler.Script<IT3ModelViewPresenter>();
				presenter.Textures["leftLine"].ColorModifier.Update();
				presenter.Textures["rightLine"].ColorModifier.Update();
				presenter.Textures["main"].SortingOrderModifier.Update();
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			var info = manageSystem.LayersInfo.Value;
			if (info is not null) info.OnDataUpdated -= OnLayerUpdate;
			ResetLayerView();
		}
	}
}