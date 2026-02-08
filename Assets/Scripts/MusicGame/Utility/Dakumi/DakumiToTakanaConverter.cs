#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.JudgeLine;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Static.Easing;
using T3Framework.Static.Movement;

namespace MusicGame.Utility.Dakumi
{
	public static class DakumiToTakanaConverter
	{
		public static ChartInfo DeserializeFromDakumi(DakumiChart dakumiChart)
		{
			var chart = new ChartInfo();
			chart.SetOffsetInfo(-dakumiChart.Offset); // Caution: offset is reversed
			var line = chart.AddComponent(new StaticJudgeLine());
			// Add track meta
			foreach (var (id, trackInfo) in dakumiChart.Tracks)
			{
				var track = GetsTrackById(chart, id, line);
				track.Name = trackInfo.Name;
				track.Model.SetIsShowWhen0(trackInfo.ShowWhen0);
				if (trackInfo.Type == "lposrpos")
				{
					var movement = new TrackEdgeMovement(
						track.Model.Movement.Movement1, track.Model.Movement.Movement2);
					track.Model.Movement = movement;
				}
			}

			// Add event and confirming track's start and end time
			foreach (var (id, list) in dakumiChart.Events)
			{
				var track = GetsTrackById(chart, id, line);
				foreach (var d3Event in list)
				{
					var timeStart = GetTime(dakumiChart.BpmList, d3Event.StartBeat);
					var timeEnd = GetTime(dakumiChart.BpmList, d3Event.EndBeat);

					track.Model.TimeStart = Math.Min(track.Model.TimeStart, timeStart);
					var movement = d3Event.Type == 1 ? track.Model.Movement.Movement1 : track.Model.Movement.Movement2;
					if (movement is ChartPosMoveList moveList)
					{
						var isWidth =
							(!dakumiChart.Tracks.TryGetValue(id, out var trackInfo) || trackInfo.Type == "xw") &&
							d3Event.Type == 2;
						var from = isWidth
							? ConvertWidth(d3Event.From, dakumiChart)
							: ConvertPos(d3Event.From, dakumiChart);
						var to = isWidth
							? ConvertWidth(d3Event.To, dakumiChart)
							: ConvertPos(d3Event.To, dakumiChart);

						IPositionMoveItem<float> moveItem = d3Event.Trans.Type == "bezier"
							? new V1BMoveItem(from,
								new(d3Event.Trans.BezierTrans[0], d3Event.Trans.BezierTrans[1]),
								new(d3Event.Trans.BezierTrans[2], d3Event.Trans.BezierTrans[3]))
							: new V1EMoveItem(from, d3Event.Trans.Easing.Opposite());
						InsertWithCollision(moveList, timeStart, moveItem, 1);
						InsertWithCollision(moveList, timeEnd, new V1EMoveItem(to, Eases.Unmove), -1);
					}
				}
			}

			// Add Note
			foreach (var note in dakumiChart.Notes)
			{
				var track = GetsTrackById(chart, note.Track, line);
				foreach (var t3Note in FromNote(note, dakumiChart))
				{
					if (t3Note.TimeMax < track.Model.TimeMin) continue;
					chart.AddComponent(t3Note).Parent = track;
				}
			}

			var tracks = chart.Where(c => c.Model is Track).ToList();
			foreach (var track in tracks)
			{
				if (track.Model.TimeMin == T3Time.MaxValue) chart.RemoveComponent(track);
				else if (track.Model is Track model) model.TimeEnd = T3Time.MaxValue;
			}

			return chart;
		}

		public static ChartComponent<Track> GetsTrackById(ChartInfo chart, int trackId, ChartComponent root)
		{
			foreach (var component in root.Children)
			{
				if (component.Id == trackId && component is ChartComponent<Track> c) return c;
			}

			var model = new Track(T3Time.MaxValue, T3Time.MinValue) // Initial illegal value
			{
				Movement = new TrackDirectMovement(new ChartPosMoveList(), new ChartPosMoveList())
			};
			var track = chart.AddComponentGeneric(model);
			track.Parent = root;
			track.Id = trackId;
			return track;
		}

		public static int GetTime(SortedDictionary<Beat, float> bpmList, Beat targetBeat)
		{
			if (bpmList.Count == 0) return 0;

			double totalMs = 0;
			double targetTotalBeats = targetBeat.GetTotalBeats();

			var nodes = bpmList.ToList();
			for (int i = 0; i < nodes.Count; i++)
			{
				var currentBeat = nodes[i].Key;
				var currentBpm = nodes[i].Value;
				double currentTotalBeats = currentBeat.GetTotalBeats();

				if (targetTotalBeats <= currentTotalBeats) break;

				double nextTotalBeats;
				if (i + 1 < nodes.Count)
				{
					double nextNodeBeats = nodes[i + 1].Key.GetTotalBeats();
					nextTotalBeats = Math.Min(targetTotalBeats, nextNodeBeats);
				}
				else
				{
					nextTotalBeats = targetTotalBeats;
				}

				double deltaBeats = nextTotalBeats - currentTotalBeats;
				totalMs += deltaBeats * (60000.0 / currentBpm);

				if (nextTotalBeats >= targetTotalBeats) break;
			}

			return (int)Math.Round(totalMs);
		}

		public static IEnumerable<INote> FromNote(Note note, DakumiChart dakumiChart)
		{
			var startTime = GetTime(dakumiChart.BpmList, note.StartBeat);
			INote t3Note = note.Type switch
			{
				NoteType.Note or NoteType.Wipe => new Hit(
					startTime,
					note.Type switch
					{
						NoteType.Wipe => HitType.Slide,
						NoteType.Note => HitType.Tap,
						_ => HitType.Tap
					}),
				NoteType.Hold => new Hold(
					startTime,
					GetTime(dakumiChart.BpmList, note.EndBeat)),
				_ => throw new ArgumentOutOfRangeException()
			};

			t3Note.SetDummy(note.IsFake);
			yield return t3Note;

			if (note.Type == NoteType.Hold)
			{
				if (note.WithNoteHead)
				{
					var noteHead = new Hit(startTime, HitType.Tap);
					noteHead.SetDummy(note.IsFake);
					yield return noteHead;
				}

				if (note.WithWipeHead)
				{
					var wipeHead = new Hit(startTime, HitType.Slide);
					wipeHead.SetDummy(note.IsFake);
					yield return wipeHead;
				}
			}
		}

		public static void InsertWithCollision(
			ChartPosMoveList moveList, T3Time time, IPositionMoveItem<float> toInsert, T3Time addIfCollide)
		{
			while (addIfCollide != 0 && moveList.TryGet(time, out _)) time += addIfCollide;
			moveList.Insert(time, toInsert);
		}

		public static float ConvertPos(float pos, DakumiChart dakumiChart)
			=> (pos + dakumiChart.Preference.XOffset) / dakumiChart.Preference.EventScale * 9 - 4.5f;

		public static float ConvertWidth(float width, DakumiChart dakumiChart)
			=> width / dakumiChart.Preference.EventScale * 9;
	}
}