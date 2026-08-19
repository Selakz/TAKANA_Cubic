#nullable enable

using System;
using EditorPlugin.PluginSystem;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.EditPanel.Commands;
using MusicGame.ChartEditor.TrackLayer;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Static.Movement;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared.To
{
	public class RawTrackData
	{
		public string type { get; }

		public IWrapper<int> id { get; }

		public IWrapper<string?> name { get; }

		public IWrapper<int> timeStart { get; }

		public IWrapper<int> timeEnd { get; }

		public object movement { get; }

		private readonly ChartComponent component;
		private readonly StagingRegistry? registry;

		public RawTrackData(ChartComponent component, StagingRegistry? registry)
		{
			this.component = component;
			this.registry = registry;
			type = "Track";
			id = new ValueWrapper<int>(() => component.Id, v => component.Id = v, registry);
			name = new ValueWrapper<string?>(() => component.Name, v => component.Name = v, registry);
			timeStart = new TrackTimeWrapper(component, registry, isStart: true);
			timeEnd = new TrackTimeWrapper(component, registry, isStart: false);
			movement = ((ITrack)component.Model).Movement switch
			{
				TrackEdgeMovement => new RawTrackEdgeMovement(component, registry),
				TrackDirectMovement => new RawTrackDirectMovement(component, registry),
				_ => throw new InvalidOperationException(
					$"Unsupported track movement: {((ITrack)component.Model).Movement.GetType()}")
			};
		}

		public RawLayerData getLayer()
		{
			return component.GetLayerInfo() is { } info
				? new RawLayerData(info)
				: throw new InvalidOperationException("Track has no layer info.");
		}

		public void setLayer(int id)
		{
			if (component.Model is not ITrack model) return;
			if (registry is null) component.UpdateModel(m => ((ITrack)m).SetLayer(id));
			else
			{
				registry.Add(() => new UpdateValueCommand<int>(
					() => model.GetLayerId(),
					v =>
					{
						model.SetLayer(v);
						component.UpdateNotify();
					},
					id));
			}
		}

		public void nudge(int distance)
		{
			Debug.Log($"Nudge track, registry: {registry is null}");
			if (registry is null) component.Nudge(distance);
			else
			{
				registry.Add(() => new AddValueCommand<T3Time>(
					() => component.Model.TimeMin,
					v => component.Nudge(v - component.Model.TimeMin),
					distance,
					(a, b) => a + b));
			}
		}

		public void shift(float offset)
		{
			if (registry is null) component.UpdateModel(m => ((ITrack)m).Shift(offset));
			else
			{
				registry.Add(() => new AddValueCommand<float>(
					() => ((ITrack)component.Model).Movement.GetPos(component.Model.TimeMin),
					v =>
					{
						var model = (ITrack)component.Model;
						model.Shift(v - model.Movement.GetPos(component.Model.TimeMin));
						component.UpdateNotify();
					},
					offset,
					(a, b) => a + b));
			}
		}

		public static ChartPosMoveList NewMoveList() => new();

		// Use this method instead of ChartPosMoveList.Insert because puerTS will call Insert(T3Time, float) even when provided param is IPositionMoveItem<float>.
		// Who to blame? Me??
		public static void Insert(ChartPosMoveList list, T3Time time, IPositionMoveItem<float> item) =>
			list.Insert(time, item);
	}

	public class TrackTimeWrapper : IWrapper<int>
	{
		private readonly ChartComponent component;
		private readonly StagingRegistry? registry;
		private readonly bool isStart;
		private int stagedValue;
		private bool dirty;

		public TrackTimeWrapper(ChartComponent component, StagingRegistry? registry, bool isStart)
		{
			this.component = component;
			this.registry = registry;
			this.isStart = isStart;
			stagedValue = default;
		}

		public int value
		{
			get
			{
				var model = (ITrack)component.Model;
				return isStart ? model.TimeStart.Milli : model.TimeEnd.Milli;
			}
			set
			{
				if (!IsValid(value)) return;

				if (registry is null)
				{
					new UpdateTrackTimeCommand(component, isStart, new T3Time(value)).Do();
					return;
				}

				if (!dirty)
				{
					registry.Add(Factory);
					dirty = true;
				}

				stagedValue = value;
				return;

				bool IsValid(int candidate)
				{
					var time = new T3Time(candidate);
					return isStart ? component.IsNewTimeMinValid(time) : component.IsNewTimeMaxValid(time);
				}
			}
		}

		public ICommand Factory()
		{
			var commandValue = stagedValue;
			dirty = false;
			stagedValue = default;
			return new UpdateTrackTimeCommand(component, isStart, new T3Time(commandValue));
		}

		public void Dispose() => registry?.Remove(Factory);
	}
}