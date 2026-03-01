#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.Utility.UI
{
	public class ChartFormatter : HierarchySystem<ChartFormatter>
	{
		// Serializable and Public
		[SerializeField] private Toggle removeOutOfRangeToggle = default!;
		[SerializeField] private Toggle removeOverlapNoteToggle = default!;
		[SerializeField] private Toggle removeZeroWidthTrackNoteToggle = default!;
		[SerializeField] private Button executeButton = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(executeButton, ExecuteSelectedFunctions)
		};

		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private CommandManager commandManager = default!;

		// Constructor
		[Inject]
		private void Construct(
			NotifiableProperty<LevelInfo?> levelInfo,
			CommandManager commandManager)
		{
			this.levelInfo = levelInfo;
			this.commandManager = commandManager;
		}

		private void ExecuteSelectedFunctions()
		{
			if (levelInfo.Value?.Chart is not { } chart) return;

			int count = 0;
			count += removeOutOfRangeToggle.isOn ? RemoveOutOfRangeComponents(chart) : 0;
			count += removeOverlapNoteToggle.isOn ? RemoveOverlapNotes(chart) : 0;
			count += removeZeroWidthTrackNoteToggle.isOn ? RemoveZeroWidthTrackNotes(chart) : 0;

			if (count > 0)
			{
				T3Logger.Log("Notice", $"Edit_Format_FormattingSuccess|{count}", T3LogType.Success);
				commandManager.Clear();
			}
			else
			{
				T3Logger.Log("Notice", "Edit_Format_NoNeedToFormat", T3LogType.Success);
			}
		}

		private static int RemoveOutOfRangeComponents(ChartInfo chart)
		{
			List<ChartComponent> toRemove = chart.Where(component => !component.IsWithinParentRange(0)).ToList();
			foreach (var component in toRemove) chart.RemoveComponent(component);
			return toRemove.Count;
		}

		private static int RemoveOverlapNotes(ChartInfo chart)
		{
			List<ChartComponent> toRemove = new();
			var classifier = T3ChartClassifier.Instance;

			var notesByParent = chart
				.Where(c => c.Model is INote && c.Parent is not null)
				.GroupBy(c => c.Parent);

			foreach (var parentGroup in notesByParent)
			{
				var parent = parentGroup.Key;
				if (parent is null) continue;

				var overlapGroups = parentGroup.GroupBy(c =>
				{
					var type = classifier.Classify(c);
					return (type, c.Model.TimeMin, c.Model.TimeMax);
				});

				foreach (var overlapGroup in overlapGroups)
				{
					var duplicates = overlapGroup.ToList();
					if (duplicates.Count > 1)
					{
						for (int i = 1; i < duplicates.Count; i++) toRemove.Add(duplicates[i]);
					}
				}
			}

			foreach (var component in toRemove) chart.RemoveComponent(component);
			return toRemove.Count;
		}

		private static int RemoveZeroWidthTrackNotes(ChartInfo chart)
		{
			List<ChartComponent> toRemove = new();

			foreach (var component in chart)
			{
				if (component.Model is not INote note) continue;
				if (component.Parent?.Model is not ITrack track) continue;
				if (Mathf.Approximately(track.Movement.GetWidth(note.TimeMin), 0)) toRemove.Add(component);
			}

			foreach (var component in toRemove) chart.RemoveComponent(component);
			return toRemove.Count;
		}
	}
}