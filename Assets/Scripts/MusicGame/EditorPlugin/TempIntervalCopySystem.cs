#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.EditorPlugin
{
	public class TempIntervalCopySystem : HierarchySystem<TempIntervalCopySystem>
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField startTimeInput = default!;
		[SerializeField] private TMP_InputField endTimeInput = default!;
		[SerializeField] private TMP_InputField targetTimeInput = default!;
		[SerializeField] private Button executeButton = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(executeButton, () =>
			{
				if (!int.TryParse(startTimeInput.text, out int startTime) ||
				    !int.TryParse(endTimeInput.text, out int endTime) ||
				    !int.TryParse(targetTimeInput.text, out int targetTime) ||
				    levelInfo.Value is not { } info) return;
				commandManager.Add(IntervalCopy(info.Chart, startTime, endTime, targetTime));
			})
		};

		// Private
		[Inject] private NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private CommandManager commandManager = default!;

		// Defined Functions
		/// <summary>
		/// Copy the interval of the chart to the target time.
		/// </summary>
		public ICommand IntervalCopy(ChartInfo chart, T3Time startTime, T3Time endTime, T3Time targetTime)
		{
			List<ChartComponent> newTracks = new();

			foreach (var component in chart)
			{
				if (component.Model is not ITrack trackModel) continue;
				T3Time trackStart = trackModel.TimeStart;
				T3Time trackEnd = trackModel.TimeEnd;

				if (trackEnd <= startTime || trackStart >= endTime) continue;
				if (trackStart >= startTime && trackEnd <= endTime)
				{
					Track clonedTrack = IChartSerializable.Clone((Track)trackModel);
					ChartComponent clonedComponent = new(clonedTrack);

					foreach (var child in component.Children)
					{
						var clonedChild = IChartSerializable.Clone(child.Model);
						_ = new ChartComponent(clonedChild) { Parent = clonedComponent };
					}

					newTracks.Add(clonedComponent);
				}
				else
				{
					Track clonedTrack = IChartSerializable.Clone((Track)trackModel);
					clonedTrack.TimeStart = Mathf.Max(startTime, trackStart);
					clonedTrack.TimeEnd = Mathf.Min(endTime, trackEnd);

					if (clonedTrack.Movement.Movement1 is ChartPosMoveList newMovement1)
					{
						foreach (var item in newMovement1
							         .Where(item => item.Key < clonedTrack.TimeStart || item.Key > clonedTrack.TimeEnd)
							         .ToList())
						{
							newMovement1.Remove(item.Key);
						}
					}

					if (clonedTrack.Movement.Movement2 is ChartPosMoveList newMovement2)
					{
						foreach (var item in newMovement2
							         .Where(item => item.Key < clonedTrack.TimeStart || item.Key > clonedTrack.TimeEnd)
							         .ToList())
						{
							newMovement2.Remove(item.Key);
						}
					}

					float startPos = trackModel.Movement.GetPos(clonedTrack.TimeStart);
					float startWidth = trackModel.Movement.GetWidth(clonedTrack.TimeStart);
					clonedTrack.Movement.Insert(clonedTrack.TimeStart, startPos, startWidth);

					float endPos = trackModel.Movement.GetPos(clonedTrack.TimeEnd);
					float endWidth = trackModel.Movement.GetWidth(clonedTrack.TimeEnd);
					clonedTrack.Movement.Insert(clonedTrack.TimeEnd, endPos, endWidth);

					ChartComponent clonedComponent = new(clonedTrack);

					foreach (var child in component.Children)
					{
						if (child.Model.TimeMin >= clonedTrack.TimeStart && child.Model.TimeMax <= clonedTrack.TimeEnd)
						{
							var clonedChild = IChartSerializable.Clone(child.Model);
							_ = new ChartComponent(clonedChild) { Parent = clonedComponent };
						}
					}

					newTracks.Add(clonedComponent);
				}
			}

			T3Time distance = targetTime - startTime;
			foreach (var track in newTracks) track.Nudge(distance);

			var defaultJudgeLine = chart.DefaultJudgeLine();
			List<ICommand> commands = newTracks
				.Select(track => new AddComponentCommand(chart, track, defaultJudgeLine))
				.Cast<ICommand>().ToList();

			return new BatchCommand(commands, "Interval Copy");
		}
	}
}