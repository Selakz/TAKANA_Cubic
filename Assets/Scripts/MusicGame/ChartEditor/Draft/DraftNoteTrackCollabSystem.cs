#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Easing;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.Draft
{
	public class DraftNoteTrackCollabSystem : HierarchySystem<DraftNoteTrackCollabSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(draftContainer.IsInDraftMode, isInDraftMode => IsEnabled = isInDraftMode)
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("Draft", "TrackAnchor", () =>
			{
				var track = selectDataset.FirstOrDefault(x => x.Model is ITrack);
				var draftNotes = selectDataset.Where(x => x.Model is ISolitaryNote).ToList();
				if (track is not { Model: ITrack model } ||
				    draftNotes.Count == 0 || selectDataset.Count != draftNotes.Count + 1)
				{
					T3Logger.Log("Notice", "Edit_Draft_TrackAnchorInvalid", T3LogType.Info);
					return;
				}

				List<UpdateMoveListArg> args = new();
				switch (model.Movement)
				{
					case TrackDirectMovement:
						foreach (var draftNote in draftNotes)
						{
							if (draftNote.Model is not ISolitaryNote draft) continue;
							var posArg = new UpdateMoveListArg(true, null,
								new(draft.TimeMin, new V1EMoveItem(draft.Position, Eases.Unmove)));
							var widthArg = new UpdateMoveListArg(false, null,
								new(draft.TimeMin, new V1EMoveItem(draft.Width, Eases.Unmove)));
							args.Add(posArg);
							args.Add(widthArg);
						}

						break;
					case TrackEdgeMovement:
						foreach (var draftNote in draftNotes)
						{
							if (draftNote.Model is not ISolitaryNote draft) continue;
							var leftArg = new UpdateMoveListArg(true, null,
								new(draft.TimeMin, new V1EMoveItem(draft.Position - draft.Width / 2, Eases.Unmove)));
							var rightArg = new UpdateMoveListArg(false, null,
								new(draft.TimeMin, new V1EMoveItem(draft.Position + draft.Width / 2, Eases.Unmove)));
							args.Add(leftArg);
							args.Add(rightArg);
						}

						break;
				}

				var updateMoveListCommand = new UpdateMoveListCommand(args);
				if (!updateMoveListCommand.SetInit(track))
				{
					T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
					return;
				}

				BatchCommand command = new BatchCommand(draftNotes
						.Select(x => new DeleteComponentCommand(x))
						.Concat(Enumerable.Repeat<ICommand>(updateMoveListCommand, 1)),
					"Use draft note as track's movement");
				commandManager.Add(command);
			}),
			new InputRegistrar("Draft", "CreateTrack", () =>
			{
				if (selectDataset.Any(x => x.Model is not ISolitaryNote))
				{
					T3Logger.Log("Notice", "Edit_Draft_CreateTrackInvalid", T3LogType.Info);
					return;
				}

				var createTrackCommands = selectDataset
					.Select(x =>
					{
						var model = (x.Model as ISolitaryNote)!;
						var timeStart = timeRetriever.TimeRetriever.Value switch
						{
							GridTimeRetriever grid => grid.GetFloorTime(model.TimeMin),
							_ => model.TimeMin - 1000
						};
						var timeEnd = model.TimeMax;
						var left = model.Position - model.Width / 2;
						var right = model.Position + model.Width / 2;
						return service.CreateTrack(timeStart, timeEnd, left, right);
					})
					.Where(x => x is not null)
					.Cast<ICommand>();
				commandManager.Add(new BatchCommand(
					createTrackCommands.Concat(selectDataset.Select(x => new DeleteComponentCommand(x))),
					"Create track from draft notes"));
			}),
			new InputRegistrar("Draft", "AttachNote", () =>
			{
				var draftNotes = selectDataset.Where(x => x.Model is ISolitaryNote).ToList();
				if (draftNotes.Count == 0) return;
				var selectedTracks = selectDataset.Where(x => x.Model is ITrack).ToList();
				bool hasOnlyDraftNotes = draftNotes.Count == selectDataset.Count;
				bool hasTracksAndDraftNotes = selectedTracks.Count > 0 &&
				                              draftNotes.Count + selectedTracks.Count == selectDataset.Count;
				if (!hasOnlyDraftNotes && !hasTracksAndDraftNotes) return;

				ChartComponent[] trackSource = noteParentService.GetAvailableTracks(hasOnlyDraftNotes
					? levelInfo.Value?.Chart.Where(x => x.Model is ITrack) ?? Enumerable.Empty<ChartComponent>()
					: selectedTracks).ToArray();

				List<ICommand> commands = new();
				bool addedLog = false;
				foreach (var note in draftNotes)
				{
					var model = (note.Model as ISolitaryNote)!;
					// 1. Get all track whose lifetime overlaps the notes' lifetime.
					var tracks = trackSource.Where(x =>
						x.Model is ITrack track &&
						track.TimeMin <= note.Model.TimeMin && track.TimeMax >= note.Model.TimeMax);
					// 2. Find the track whose position and width at the note's timeMin is the most similar to the note's position and width
					ChartComponent? nearestTrack = null;
					float score = float.MaxValue; // 0 means the best
					foreach (var track in tracks)
					{
						var trackModel = (track.Model as ITrack)!;
						const float weight = 0.75f;
						var posDistance = Mathf.Abs(model.Position - trackModel.Movement.GetPos(model.TimeMin));
						var widthDistance = Mathf.Abs(model.Width - trackModel.Movement.GetWidth(model.TimeMin));
						var distance = posDistance * weight + widthDistance * (1 - weight);
						if (distance < score)
						{
							score = distance;
							nearestTrack = track;
						}
					}

					// 3. Attach the draft to it
					if (nearestTrack is null)
					{
						if (addedLog) continue;
						addedLog = true;
						commands.Add(new LogCommand("Notice", "Edit_Draft_AttachNoteFail", T3LogType.Warn));
					}
					else
					{
						var newModel = model.ToSimpleNote();
						commands.Add(new AddComponentCommand(nearestTrack.BelongingChart!, newModel, nearestTrack));
						commands.Add(new DeleteComponentCommand(note));
					}
				}

				commandManager.Add(new BatchCommand(commands, "Attach draft notes to track"));
			})
		};

		// Private
		[Inject] private DraftContainer draftContainer = default!;
		[Inject] private ChartSelectDataset selectDataset = default!;
		[Inject] private ChartEditSystem service = default!;
		[Inject] private StageMouseTimeRetriever timeRetriever = default!;
		[Inject] private CommandManager commandManager = default!;
		[Inject] private NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private INoteParentService noteParentService = default!;
	}
}