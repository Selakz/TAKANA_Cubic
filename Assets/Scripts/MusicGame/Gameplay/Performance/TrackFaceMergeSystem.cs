#nullable enable

using System.Collections.Generic;
using MusicGame.Gameplay.Basic;
using MusicGame.Gameplay.Basic.T3;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

namespace MusicGame.Gameplay.Performance
{
	public class TrackFaceMergeSystem : HierarchySystem<TrackFaceMergeSystem>
	{
		// Serializable and Public
		[FormerlySerializedAs("simpleTrackPrefab")] [SerializeField]
		private PrefabObject trackPrefab = default!;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(ISingleton<PerformanceSetting>.Instance.MergeTrackFace,
				value => IsEnabled = value)
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolRegistrar<ChartComponent>(trackPool,
				ViewPoolRegistrar<ChartComponent>.RegisterTarget.Get,
				handler =>
				{
					var presenter = handler.Script<T3TrackViewPresenter>();
					presenter.MainTexture.Value.gameObject.SetActive(false);
				})
		};

		// Private
		private IObjectResolver resolver = default!;
		private IViewPool<ChartComponent> trackPool = default!;

		private readonly List<PrefabHandler> handlers = new();
		private readonly List<Interval> intervals = new();

		// Constructor
		[Inject]
		private void Construct(
			IObjectResolver resolver,
			[Key("stage")] IViewPool<ChartComponent> viewPool)
		{
			this.resolver = resolver;
			trackPool = new SubViewPool<ChartComponent, T3Flag>(viewPool, new T3ChartClassifier(), T3Flag.Track);
		}

		// System Functions
		void LateUpdate() // Wait for real tracks to calculate their positions
		{
			int count = 0;
			foreach (var track in trackPool)
			{
				if (trackPool[track] is not { } handler) continue;
				var position = handler.Script<ITrackViewPresenter>().PositionModifier.Value;
				var width = handler.Script<ITrackViewPresenter>().WidthModifier.Value;
				if (Mathf.Approximately(width, 0)) continue;
				var interval = new Interval(position - width / 2, position + width / 2);
				if (intervals.Count == count) intervals.Add(interval);
				else intervals[count] = interval;
				count++;
			}

			var mergedCount = IntervalMerger.Merge(intervals, count);
			for (int i = 0; i < mergedCount; ++i)
			{
				if (handlers.Count == i) handlers.Add(trackPrefab.Instantiate(resolver, trackPool.DefaultTransform));
				else handlers[i].gameObject.SetActive(true);
				var presenter = handlers[i].Script<ITrackViewPresenter>();
				presenter.PositionModifier.Assign((intervals[i].Left + intervals[i].Right) / 2, 1);
				presenter.WidthModifier.Assign(intervals[i].Right - intervals[i].Left, 1);
			}

			for (int i = mergedCount; i < handlers.Count; ++i)
			{
				handlers[i].gameObject.SetActive(false);
			}
		}
	}
}