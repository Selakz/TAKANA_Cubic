#nullable enable

using System.Linq;
using DLCore.Models;
using DLCore.Models.Note;
using DLCore.Models.Track;
using DLCore.Models.Track.Movement;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Static.Movement;
using UnityEngine;

namespace DLCore.Utility.Takana
{
	public static class TakanaToDLConverter
	{
		private const float SampleStepSeconds = 0.25f;
		private const float MinColorDurationSeconds = 3f;

		public static ChartInfo Convert(ChartInfo takanaChart)
		{
			ChartInfo result = new()
			{
				Properties = IChartSerializable.Clone(takanaChart.Properties),
				EditorConfig = IChartSerializable.Clone(takanaChart.EditorConfig)
			};
			foreach (var root in takanaChart.Where(component => component.Parent is null))
			{
				ConvertComponent(root, result, null, null);
			}

			return result;
		}

		private static void ConvertComponent(
			ChartComponent source, ChartInfo target, ChartComponent? targetParent,
			IMovement<ColorVariantGradient>? trackColorMovement)
		{
			switch (source.Model)
			{
				case Track track:
				{
					var colorMovement = BuildColorMovement(track);
					var model = new DLTrack(track.TimeStart, track.TimeEnd)
					{
						Movement = IChartSerializable.Clone(track.Movement),
						ColorMovement = colorMovement,
						Properties = IChartSerializable.Clone(track.Properties),
						EditorConfig = IChartSerializable.Clone(track.EditorConfig)
					};
					AddConverted(source, model, target, targetParent, out var component);
					foreach (var child in source.Children) ConvertComponent(child, target, component, colorMovement);
					return;
				}
				case DraftHit draftHit:
					ConvertDraft(source, draftHit, target, targetParent);
					return;
				case DraftHold draftHold:
					ConvertDraft(source, draftHold, target, targetParent);
					return;
				case Hit hit:
				{
					var model = new DLHit(hit.TimeJudge, hit.Type,
						GetNoteColor(trackColorMovement, hit.TimeJudge))
					{
						Movement = IChartSerializable.Clone(hit.Movement),
						Properties = IChartSerializable.Clone(hit.Properties),
						EditorConfig = IChartSerializable.Clone(hit.EditorConfig)
					};
					AddConverted(source, model, target, targetParent, out _);
					return;
				}
				case Hold hold:
				{
					var model = new DLHold(hold.TimeJudge, hold.TimeEnd,
						GetNoteColor(trackColorMovement, hold.TimeJudge))
					{
						Movement = IChartSerializable.Clone(hold.Movement),
						TailMovement = IChartSerializable.Clone(hold.TailMovement),
						Properties = IChartSerializable.Clone(hold.Properties),
						EditorConfig = IChartSerializable.Clone(hold.EditorConfig)
					};
					AddConverted(source, model, target, targetParent, out _);
					return;
				}
				default:
				{
					var model = IChartSerializable.Clone(source.Model);
					AddConverted(source, model, target, targetParent, out var component);
					foreach (var child in source.Children) ConvertComponent(child, target, component, trackColorMovement);
					return;
				}
			}
		}

		private static void ConvertDraft(ChartComponent source, DraftHit draft, ChartInfo target,
			ChartComponent? targetParent)
		{
			var model = new DLFreeHit(draft.TimeJudge, draft.Type, ColorVariant.Gray)
			{
				Movement = IChartSerializable.Clone(draft.Movement),
				HorizontalMovement = CreateConstantMovement(draft.Position, draft.Width, draft.TimeJudge, draft.TimeMax),
				Properties = IChartSerializable.Clone(draft.Properties),
				EditorConfig = IChartSerializable.Clone(draft.EditorConfig)
			};
			AddConverted(source, model, target, targetParent, out _);
		}

		private static void ConvertDraft(ChartComponent source, DraftHold draft, ChartInfo target,
			ChartComponent? targetParent)
		{
			var model = new DLFreeHold(draft.TimeJudge, draft.TimeEnd, ColorVariant.Gray)
			{
				Movement = IChartSerializable.Clone(draft.Movement),
				TailMovement = IChartSerializable.Clone(draft.TailMovement),
				HorizontalMovement = CreateConstantMovement(draft.Position, draft.Width, draft.TimeJudge, draft.TimeMax),
				Properties = IChartSerializable.Clone(draft.Properties),
				EditorConfig = IChartSerializable.Clone(draft.EditorConfig)
			};
			AddConverted(source, model, target, targetParent, out _);
		}

		private static void AddConverted(
			ChartComponent source, IChartModel model, ChartInfo target, ChartComponent? targetParent,
			out ChartComponent component)
		{
			component = target.AddComponent(model);
			component.Name = source.Name;
			if (targetParent is not null) component.SetParent(targetParent);
		}

		private static ITrackMovement CreateConstantMovement(float position, float width, T3Time startTime, T3Time endTime)
		{
			ChartPosMoveList positionList = new();
			positionList.Insert(startTime, position);
			positionList.Insert(endTime, position);
			ChartPosMoveList widthList = new();
			widthList.Insert(startTime, width);
			widthList.Insert(endTime, width);
			return new TrackDirectMovement(positionList, widthList);
		}

		private static BasicColorMoveList BuildColorMovement(Track track)
		{
			BasicColorMoveList colorList = new();
			var initialColor = PositionToColor(track.Movement.GetPos(track.TimeStart));
			colorList.Insert(track.TimeStart, initialColor);

			var duration = (track.TimeEnd - track.TimeStart).Second;
			int sampleCount = Mathf.CeilToInt(duration / SampleStepSeconds);
			var runColor = initialColor;
			T3Time runStart = track.TimeStart;
			int runStartIndex = 0;
			for (int i = 1; i <= sampleCount; i++)
			{
				var time = track.TimeStart + SampleStepSeconds * i;
				var color = PositionToColor(track.Movement.GetPos(time));
				if (color != runColor)
				{
					if ((i - 1 - runStartIndex) * SampleStepSeconds >= MinColorDurationSeconds &&
					    colorList.GetPos(runStart).ToColor != runColor)
					{
						colorList.Insert(runStart, runColor);
					}

					runColor = color;
					runStart = time;
					runStartIndex = i;
				}
			}

			if ((sampleCount - runStartIndex) * SampleStepSeconds >= MinColorDurationSeconds &&
			    colorList.GetPos(runStart).ToColor != runColor)
			{
				colorList.Insert(runStart, runColor);
			}

			return colorList;
		}

		private static ColorVariant PositionToColor(float position) => position switch
		{
			< -1f => ColorVariant.Blue,
			> 1f => ColorVariant.Red,
			_ => ColorVariant.Purple
		};

		private static ColorVariant GetNoteColor(IMovement<ColorVariantGradient>? trackColorMovement, T3Time noteTime)
			=> trackColorMovement?.GetPos(noteTime).ToColor ?? ColorVariant.Gray;
	}
}