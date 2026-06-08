#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Static.Movement;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.Commands
{
	public class SplitTrackCommand : ISetInitCommand
	{
		public string Name => $"Split track {track.Id} at time {splitTime}";

		private readonly ChartInfo? chart;
		private readonly ChartComponent track;
		private readonly T3Time splitTime;

		private ITrack? trackModel;

		private T3Time originalTimeStart;
		private T3Time originalTimeEnd;

		private ChartPosMoveList? originalMovement1;
		private ChartPosMoveList? originalMovement2;

		private readonly List<ChartComponent> childrenToMove = new();
		private readonly List<KeyValuePair<T3Time, IPositionMoveItem<float>>> movedItems1 = new();
		private readonly List<KeyValuePair<T3Time, IPositionMoveItem<float>>> movedItems2 = new();

		private IPositionMoveItem<float>? originalReplacedItem1;
		private IPositionMoveItem<float>? originalReplacedItem2;

		private ChartComponent? newTrackComponent;
		private bool hasExecuted = false;

		public SplitTrackCommand(ChartComponent track, T3Time splitTime)
		{
			this.track = track;
			this.splitTime = splitTime;
			chart = track.BelongingChart;
		}

		public bool SetInit()
		{
			if (hasExecuted)
			{
				Debug.LogWarning("The command has already set init.");
				return false;
			}

			if (track.BelongingChart is null) return false;
			if (track.Model is not ITrack model) return false;
			trackModel = model;

			if (splitTime <= trackModel.TimeStart || splitTime >= trackModel.TimeEnd) return false;

			if (trackModel.Movement.Movement1 is not ChartPosMoveList movement1 ||
			    trackModel.Movement.Movement2 is not ChartPosMoveList movement2)
			{
				Debug.LogWarning("Movement1 and Movement2 must be ChartPosMoveList.");
				return false;
			}

			originalTimeStart = trackModel.TimeStart;
			originalTimeEnd = trackModel.TimeEnd;
			originalMovement1 = movement1;
			originalMovement2 = movement2;

			childrenToMove.Clear();
			foreach (var child in track.Children)
			{
				if (child.Model.TimeMin >= splitTime)
				{
					childrenToMove.Add(child);
				}
			}

			return true;
		}

		public void Do()
		{
			if (trackModel == null || originalMovement1 == null || originalMovement2 == null)
			{
				Debug.LogError("SplitTrackCommand is not properly initialized.");
				return;
			}

			hasExecuted = true;

			float splitPos = trackModel.Movement.GetPos(splitTime);
			float splitWidth = trackModel.Movement.GetWidth(splitTime);

			// 1. create new track component
			if (newTrackComponent is null)
			{
				var newTrack = IChartSerializable.Clone((Track)trackModel);
				newTrack.TimeStart = splitTime;
				newTrack.TimeEnd = originalTimeEnd;
				newTrackComponent = new ChartComponent(newTrack);

				if (newTrack.Movement.Movement1 is ChartPosMoveList newMovement1)
				{
					foreach (var item in newMovement1.ToList())
					{
						if (item.Key < splitTime) newMovement1.Remove(item.Key);
					}
				}

				if (newTrack.Movement.Movement2 is ChartPosMoveList newMovement2)
				{
					foreach (var item in newMovement2.ToList())
					{
						if (item.Key < splitTime) newMovement2.Remove(item.Key);
					}
				}

				newTrack.Movement.Insert(splitTime, splitPos, splitWidth);
			}

			// 2. move items from splitTime onwards from original track
			movedItems1.Clear();
			movedItems2.Clear();

			originalMovement1.TryGet(splitTime, out originalReplacedItem1);
			originalMovement2.TryGet(splitTime, out originalReplacedItem2);

			foreach (var item in originalMovement1.ToList())
			{
				if (item.Key >= splitTime)
				{
					movedItems1.Add(item);
					originalMovement1.Remove(item.Key);
				}
			}

			foreach (var item in originalMovement2.ToList())
			{
				if (item.Key >= splitTime)
				{
					movedItems2.Add(item);
					originalMovement2.Remove(item.Key);
				}
			}

			trackModel.Movement.Insert(splitTime, splitPos, splitWidth);

			// 3. move children to new track
			foreach (var child in childrenToMove)
			{
				child.SetParent(newTrackComponent);
			}

			// 4. update original track time range
			track.UpdateModel(_ =>
			{
				trackModel.TimeStart = originalTimeStart;
				trackModel.TimeEnd = splitTime;
			});
			newTrackComponent.BelongingChart = chart;
		}

		public void Undo()
		{
			if (trackModel == null || originalMovement1 == null || originalMovement2 == null ||
			    newTrackComponent is null)
			{
				Debug.LogError("SplitTrackCommand is not properly initialized.");
				return;
			}

			// 1. move items back to original track
			foreach (var item in movedItems1)
			{
				originalMovement1.Insert(item.Key, item.Value);
			}

			foreach (var item in movedItems2)
			{
				originalMovement2.Insert(item.Key, item.Value);
			}

			if (originalReplacedItem1 is not null) originalMovement1.Insert(splitTime, originalReplacedItem1);
			else originalMovement1.Remove(splitTime);

			if (originalReplacedItem2 is not null) originalMovement2.Insert(splitTime, originalReplacedItem2);
			else originalMovement2.Remove(splitTime);

			// 2. update original track and remove new track from chart
			track.UpdateModel(_ =>
			{
				trackModel.TimeStart = originalTimeStart;
				trackModel.TimeEnd = originalTimeEnd;
			});
			newTrackComponent.BelongingChart = null;

			// 3. move children back to original track
			foreach (var child in childrenToMove)
			{
				child.SetParent(track);
			}
		}
	}
}