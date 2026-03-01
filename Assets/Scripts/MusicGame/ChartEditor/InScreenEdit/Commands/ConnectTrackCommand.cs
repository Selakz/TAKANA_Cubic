#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Runtime.Log;
using T3Framework.Static.Movement;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.Commands
{
	public class ConnectTrackCommand : ISetInitCommand
	{
		public string Name => $"Connect track {track1.Id} and {track2.Id} into one";

		private readonly ChartInfo? chart;
		private ChartComponent track1;
		private ChartComponent track2;

		private ITrack? trackModel1;
		private ITrack? trackModel2;

		private T3Time originalTrack1TimeStart;
		private T3Time originalTrack1TimeEnd;
		private T3Time originalTrack2TimeEnd;

		private ChartPosMoveList? track1Movement1;
		private ChartPosMoveList? track1Movement2;
		private ChartPosMoveList? track2Movement1;
		private ChartPosMoveList? track2Movement2;

		private readonly List<ChartComponent> originalTrack2Children = new();

		private readonly List<KeyValuePair<T3Time, IPositionMoveItem<float>>> movedItemsFromMovement1 = new();
		private readonly List<KeyValuePair<T3Time, IPositionMoveItem<float>>> movedItemsFromMovement2 = new();

		private bool hasExecuted = false;

		public ConnectTrackCommand(ChartComponent track1, ChartComponent track2)
		{
			this.track1 = track1;
			this.track2 = track2;
			chart = track1.BelongingChart;
		}

		public bool SetInit()
		{
			if (hasExecuted)
			{
				Debug.LogWarning("The command has already set init.");
				return false;
			}

			if (track1.BelongingChart is null || track1.BelongingChart != track2.BelongingChart) return false;
			if (track1.Model is not ITrack model1 || track2.Model is not ITrack model2) return false;

			trackModel1 = model1;
			trackModel2 = model2;

			if (IsTimeRangeOverlapping(
				    trackModel1.TimeStart, trackModel1.TimeEnd, trackModel2.TimeStart, trackModel2.TimeEnd))
			{
				T3Logger.Log("Notice", "Edit_ConnectTrackOverlapping", T3LogType.Warn);
				return false;
			}

			if (trackModel1.Movement.GetType() != trackModel2.Movement.GetType())
			{
				T3Logger.Log("Notice", "Edit_ConnectTrackTypeNotSame", T3LogType.Warn);
				return false;
			}

			if (trackModel1.Movement.Movement1 is not ChartPosMoveList movement1_1 ||
			    trackModel1.Movement.Movement2 is not ChartPosMoveList movement1_2 ||
			    trackModel2.Movement.Movement1 is not ChartPosMoveList movement2_1 ||
			    trackModel2.Movement.Movement2 is not ChartPosMoveList movement2_2)
			{
				Debug.LogWarning("Movement1 and Movement2 must be ChartPosMoveList.");
				return false;
			}

			track1Movement1 = movement1_1;
			track1Movement2 = movement1_2;
			track2Movement1 = movement2_1;
			track2Movement2 = movement2_2;

			var isTrack1Earlier = trackModel1.TimeEnd <= trackModel2.TimeStart;
			if (!isTrack1Earlier)
			{
				(track1, track2) = (track2, track1);
				(trackModel1, trackModel2) = (trackModel2, trackModel1);
				(track1Movement1, track2Movement1) = (track2Movement1, track1Movement1);
				(track1Movement2, track2Movement2) = (track2Movement2, track1Movement2);
			}

			originalTrack1TimeStart = trackModel1.TimeStart;
			originalTrack1TimeEnd = trackModel1.TimeEnd;
			originalTrack2TimeEnd = trackModel2.TimeEnd;

			originalTrack2Children.AddRange(track2.Children.ToList());
			return true;
		}

		public void Do()
		{
			if (trackModel1 == null || trackModel2 == null ||
			    track1Movement1 == null || track1Movement2 == null ||
			    track2Movement1 == null || track2Movement2 == null)
			{
				Debug.LogError("ConnectTrackCommand is not properly initialized.");
				return;
			}

			hasExecuted = true;

			// 1. add move items from track2Movement to track1Movement
			movedItemsFromMovement1.Clear();
			foreach (var item in track2Movement1)
			{
				movedItemsFromMovement1.Add(item);
				track1Movement1.Insert(item.Key, item.Value);
			}

			movedItemsFromMovement2.Clear();
			foreach (var item in track2Movement2)
			{
				movedItemsFromMovement2.Add(item);
				track1Movement2.Insert(item.Key, item.Value);
			}

			// 2. move children of track2 to track1
			foreach (var child in originalTrack2Children) child.SetParent(track1);

			// 3. update time range of track1
			track1.UpdateModel(_ =>
			{
				trackModel1.TimeStart = originalTrack1TimeStart;
				trackModel1.TimeEnd = originalTrack2TimeEnd;
			});

			track2.BelongingChart = null;
		}

		public void Undo()
		{
			if (trackModel1 == null || trackModel2 == null ||
			    track1Movement1 == null || track1Movement2 == null ||
			    track2Movement1 == null || track2Movement2 == null)
			{
				Debug.LogError("ConnectTrackCommand is not properly initialized.");
				return;
			}

			// 1. restore time range of track1
			track1.UpdateModel(_ =>
			{
				trackModel1.TimeStart = originalTrack1TimeStart;
				trackModel1.TimeEnd = originalTrack1TimeEnd;
			});

			// 2. remove move items from track2Movement in track1Movement
			foreach (var item in movedItemsFromMovement1)
			{
				track1Movement1.Remove(item.Key);
			}

			foreach (var item in movedItemsFromMovement2)
			{
				track1Movement2.Remove(item.Key);
			}

			// 3. move children of track2 back to track2
			foreach (var child in originalTrack2Children)
			{
				child.SetParent(track2);
			}

			// 4. add track2 back to chart
			track2.BelongingChart = chart;
		}

		private static bool IsTimeRangeOverlapping(T3Time start1, T3Time end1, T3Time start2, T3Time end2)
		{
			return !(end1 <= start2 || end2 <= start1);
		}
	}
}