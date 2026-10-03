#nullable enable

using System.Linq;
using MusicGame.ChartEditor.TrackLayer;
using MusicGame.Models.JudgeLine;
using MusicGame.Models.Note;
using MusicGame.Models.Track;

namespace EditorPlugin.Tests.Cases
{
	/// <summary>
	/// C# side checks of the scenarios declared by
	/// <c>Assets/StreamingAssets/Excluded/EditorPluginTests/cases/addTrack.ts</c>.
	/// Checks stay structural (counts, parents, identities) so they do not duplicate the TypeScript assertions.
	/// </summary>
	public static class AddTrackCase
	{
		[PluginTestCheck("addTrack.layer-and-callbacks")]
		public static void LayerAndCallbacks(PluginTestCheckContext ctx)
		{
			var track = ctx.Tracks.SingleOrDefault();
			ctx.Check(track is not null, "exactly one track in the chart");
			if (track is null) return;
			ctx.Check(track.Parent is not null && track.Parent.Model is IJudgeLine,
				"track parented under the judge line");
			ctx.Check(((ITrack)track.Model).GetLayerId() == ctx.Chart.GetsLayersInfo().DefaultLayer.Id,
				"layer id written to the track model");
			var note = ctx.Notes.SingleOrDefault();
			ctx.Check(note is not null, "exactly one note in the chart");
			ctx.Check(note is not null && note.Parent == track, "note parented under the track");
			ctx.Check(ctx.Commands.BatchCount == 1, "one commit produced exactly one batch");
			ctx.Check(ctx.Commands.LastBatchName == "Plugin edit", "batch name preserved");
		}

		[PluginTestCheck("addTrack.rejects-invalid-arguments")]
		public static void RejectsInvalidArguments(PluginTestCheckContext ctx)
		{
			ctx.Check(ctx.Tracks.Count() == 1, "only the valid track reached the chart");
			ctx.Check(ctx.Notes.Count() == 0, "rejected calls added no note");
			ctx.Check(ctx.Commands.BatchCount == 1, "rejected calls queued no command");
		}

		[PluginTestCheck("addTrack.callback-runs-once")]
		public static void CallbackRunsOnce(PluginTestCheckContext ctx)
		{
			ctx.Check(ctx.Tracks.Count() == 1, "track restored by redo");
			ctx.Check(ctx.Notes.Count() == 1, "note restored by redo");
			ctx.Check(ctx.Commands.BatchCount == 1, "one batch was produced");
			ctx.Check(ctx.Commands.UndoCount == 1, "one command on the undo stack after redo");
			ctx.Check(ctx.Commands.RedoCount == 0, "redo stack consumed");
		}

		[PluginTestCheck("addNote-and-addDraftNote.callbacks")]
		public static void AddNoteAndDraftNoteCallbacks(PluginTestCheckContext ctx)
		{
			var track = ctx.Tracks.SingleOrDefault();
			ctx.Check(track is not null, "exactly one track in the chart");
			ctx.Check(ctx.Notes.Count(component => component.Parent == track) == 1,
				"one note attached to the track");
			var draftNotes = ctx.Notes.Where(component => component.Model is ISolitaryNote).ToList();
			ctx.Check(draftNotes.Count == 1, "one draft note in the chart");
			ctx.Check(draftNotes.Count == 0 || draftNotes[0].Parent is { Model: IJudgeLine },
				"draft notes live under the judge line");
			ctx.Check(ctx.Notes.Count() == 2, "the rejected addNote added nothing");
		}

		[PluginTestCheck("removeComponent.staged-removal")]
		public static void StagedRemoval(PluginTestCheckContext ctx)
		{
			ctx.Check(ctx.Tracks.Count() == 0, "track removed from the chart");
			ctx.Check(ctx.Notes.Count() == 0, "note removed from the chart");
			ctx.Check(ctx.Commands.BatchCount == 3, "three commits produced three batches");
		}
	}
}
