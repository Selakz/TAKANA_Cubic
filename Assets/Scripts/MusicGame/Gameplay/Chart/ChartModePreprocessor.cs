#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.Models;
using MusicGame.Models.JudgeLine;
using MusicGame.Models.Note;
using MusicGame.Models.Note.Movement;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;

namespace MusicGame.Gameplay.Chart
{
	/// <summary>Normalizes charts of specific modes into the structure required by the editor or gameplay.</summary>
	public static class ChartModePreprocessor
	{
		private static readonly Dictionary<(string mode, bool isEditor), Action<ChartInfo>> processMap = new()
		{
			[("t3", true)] = ProcessT3Editor,
			[("t3", false)] = ProcessT3Gameplay
		};

		public static void Process(ChartInfo chart, bool isEditor)
		{
			if (processMap.TryGetValue((chart.Mode, isEditor), out var processor)) processor.Invoke(chart);
		}

		private static void ProcessT3Editor(ChartInfo chart)
		{
			// Invariants of the processed chart:
			// 1. Exactly one component whose Parent is null, and it must be a StaticJudgeLine
			//    (corresponding to T3ChartExtensions.DefaultJudgeLine()).
			// 2. Children of StaticJudgeLine can only be Track, DraftHit or DraftHold.
			// 3. Children of Track can only be Hit or Hold, and draft notes are not allowed.
			// 4. Track.Movement.Movement1/Movement2 can only be ChartPosMoveList.
			// 5. Movement of Hit/Hold must be a BaseNoteMoveList whose TimeJudge equals the note's
			//    TimeJudge and whose IsDefault() returns true.
			// 6. TailMovement of Hold must be a BaseNoteMoveList whose TimeJudge equals the Hold's
			//    TimeEnd and whose IsDefault() returns true.

			var defaultLine = chart.DefaultJudgeLine();
			// First, roots that are Track, DraftHit or DraftHold are moved under the default judge line.
			foreach (var root in chart.Where(component => component.Parent is null).ToList())
			{
				if (root.Model is Track or DraftHit or DraftHold) root.SetParent(defaultLine);
			}

			// Then, remove every component violating the invariants above, together with its descendants.
			List<ChartComponent> toRemove = chart
				.Where(component => component != defaultLine && !IsValidComponent(component))
				.ToList();
			foreach (var component in toRemove) chart.RemoveComponent(component);
			return;

			bool IsValidComponent(ChartComponent component)
			{
				return component.Parent?.Model switch
				{
					StaticJudgeLine => component.Model switch
					{
						Track track => IsTrackMovementValid(track),
						DraftHit or DraftHold => true,
						_ => false
					},
					Track => component.Model is not ISolitaryNote && component.Model is INote note &&
					         IsNoteMovementValid(note),
					_ => false
				};
			}

			bool IsTrackMovementValid(Track track)
			{
				return track.Movement switch
				{
					TrackDirectMovement movement => movement is { Movement1: ChartPosMoveList, Movement2: ChartPosMoveList },
					TrackEdgeMovement movement => movement is { Movement1: ChartPosMoveList, Movement2: ChartPosMoveList },
					_ => false
				};
			}

			bool IsNoteMovementValid(INote note)
			{
				if (note.Movement is not BaseNoteMoveList movement || movement.TimeJudge != note.TimeJudge ||
				    !movement.IsDefault()) return false;
				if (note is not Hold hold) return true;
				return hold.TailMovement is BaseNoteMoveList tailMovement &&
				       tailMovement.TimeJudge == hold.TimeEnd && tailMovement.IsDefault();
			}
		}

		private static void ProcessT3Gameplay(ChartInfo chart)
		{
			ProcessT3Editor(chart);
			// Further processing based on the former EditorLevelSaver.GetPlayableChart.
			// Callers are responsible for cloning the editing chart beforehand; here editor-only
			// components and draft components are removed, then all editor configs are cleared.
			chart.EditorConfig.Clear();
			List<ChartComponent> toRemove = new();
			foreach (var component in chart)
			{
				if (component.Model.IsEditorOnly() || T3ChartClassifier.Instance.IsOfType(component, T3Flag.Draft))
				{
					toRemove.Add(component);
					continue;
				}

				component.Model.EditorConfig.Clear();
			}

			foreach (var component in toRemove) chart.RemoveComponent(component);
		}
	}
}