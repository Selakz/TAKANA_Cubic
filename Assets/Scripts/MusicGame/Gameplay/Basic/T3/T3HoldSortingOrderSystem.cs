#nullable enable

using System.Linq;
using Cysharp.Threading.Tasks;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Collections.Generic;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Basic.T3
{
	public class T3HoldSortingOrderSystem : HierarchySystem<T3HoldSortingOrderSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority sortingOrderPriority = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, info =>
			{
				holdOrderCalculator.Clear();
				if (levelInfo.LastValue?.Chart is { } lastChart)
				{
					lastChart.OnComponentAdded -= OnComponentAdded;
					lastChart.BeforeComponentParentChanged -= BeforeComponentParentChanged;
					lastChart.OnComponentRemoved -= OnComponentRemoved;
					lastChart.OnComponentModelUpdated -= OnComponentModelUpdated;
				}

				if (info?.Chart is { } chart)
				{
					chart.OnComponentAdded += OnComponentAdded;
					chart.BeforeComponentParentChanged += BeforeComponentParentChanged;
					chart.OnComponentRemoved += OnComponentRemoved;
					chart.OnComponentModelUpdated += OnComponentModelUpdated;
					foreach (var component in chart.Where(c => c.Model is Hold))
					{
						holdOrderCalculator.Add(component);
					}
				}
			}),
			new ViewPoolLifetimeRegistrar<ChartComponent>(viewPool, handler =>
				new CustomRegistrar(
					() =>
					{
						var component = viewPool[handler]!;
						if (component.Model is not Hold) return;
						var order = holdOrderCalculator.GetLevel(component);
						var presenter = handler.Script<T3NoteViewPresenter>();
						SetHoldOrder(presenter, order);
					},
					() =>
					{
						if (handler.TryScript<T3NoteViewPresenter>() is not { } presenter) return;
						foreach (var texture in presenter.Textures.Values)
						{
							texture.SortingOrderModifier.Unregister(sortingOrderPriority);
						}
					}), true)
		};


		// Private
		[Inject] private readonly NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] [Key("stage")] private readonly IViewPool<ChartComponent> viewPool = default!;

		private SubViewPool<ChartComponent, T3Flag>? holdPool;
		private SubViewPool<ChartComponent, T3Flag> HoldPool => holdPool ??= new(viewPool, T3ChartClassifier.Instance);

		private readonly DAGLevelCalculator<ChartComponent> holdOrderCalculator = new(
			(a, b) =>
			{
				if (a.Model is not Hold aHold || b.Model is not Hold bHold) return 0;
				if (a.Parent?.Model is not ITrack aTrack || b.Parent?.Model is not ITrack bTrack) return 0;
				if (aHold.TimeMin <= bHold.TimeMin && aHold.TimeMax >= bHold.TimeMin)
				{
					return CompareInterval(bHold.TimeMin);
				}
				else if (bHold.TimeMin <= aHold.TimeMin && bHold.TimeMax >= aHold.TimeMin)
				{
					return CompareInterval(aHold.TimeMin);
				}
				else return 0;

				int CompareInterval(T3Time time)
				{
					var (aLeft, aRight) = (aTrack.Movement.GetLeftPos(time), aTrack.Movement.GetRightPos(time));
					var (bLeft, bRight) = (bTrack.Movement.GetLeftPos(time), bTrack.Movement.GetRightPos(time));
					if (aLeft <= bLeft && aRight >= bRight) return -1;
					else if (bLeft <= aLeft && bRight >= aRight) return 1;
					else return 0;
				}
			});

		// Defined Functions
		private void SetHoldOrder(T3NoteViewPresenter presenter, int order)
		{
			var count = presenter.Textures.Count;
			foreach (var texture in presenter.Textures.Values)
			{
				texture.SortingOrderModifier.Register(o => o + order * count, sortingOrderPriority);
			}
		}

		// Event Handlers
		private void OnComponentAdded(ChartComponent component)
		{
			if (component.Model is not Hold) return;
			holdOrderCalculator.Add(component);
			if (viewPool[component] is { } handler)
			{
				SetHoldOrder(handler.Script<T3NoteViewPresenter>(), holdOrderCalculator.GetLevel(component));
			}
		}

		private void BeforeComponentParentChanged(ChartComponent component, ChartComponent? lastParent)
		{
			if (component.Model is not Hold) return;
			UniTask.Yield(PlayerLoopTiming.PreLateUpdate).ToUniTask().ContinueWith(() =>
			{
				holdOrderCalculator.Remove(component);
				holdOrderCalculator.Add(component);
				if (viewPool[component] is { } handler)
				{
					SetHoldOrder(handler.Script<T3NoteViewPresenter>(), holdOrderCalculator.GetLevel(component));
				}
			});
		}

		private void OnComponentRemoved(ChartComponent component)
		{
			if (component.Model is not Hold) return;
			holdOrderCalculator.Remove(component);
		}

		private void OnComponentModelUpdated(ChartComponent component)
		{
			if (component.Model is not Hold) return;
			holdOrderCalculator.Remove(component);
			holdOrderCalculator.Add(component);
			if (viewPool[component] is { } handler)
			{
				SetHoldOrder(handler.Script<T3NoteViewPresenter>(), holdOrderCalculator.GetLevel(component));
			}
		}

		[ContextMenu("Print Orders")]
		private void PrintOrders()
		{
			foreach (var component in holdOrderCalculator.Nodes)
			{
				Debug.Log($"component: {component.Id}, order: {holdOrderCalculator.GetLevel(component)}");
			}
		}
	}
}