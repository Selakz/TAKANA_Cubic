#nullable enable

using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.Models.Track;
using T3Framework.Runtime.VContainer;

namespace MusicGame.ChartEditor.TrackLayer
{
	public class LayerNoteRawInfoService : HierarchySystem<LayerNoteRawInfoService>, INoteRawInfoService
	{
		// Serializable and Public
		public override bool AsImplementedInterfaces => true;

		// Defined Functions
		public string? IsValid(NoteRawInfo info)
		{
			if (info.Parent.Value?.Model is not ITrack model)
				return "Edit_SelectTrackToPlaceNote";

			if (info.TimeJudge.Value > info.TimeEnd.Value ||
			    info.TimeJudge.Value < model.TimeMin ||
			    info.TimeEnd.Value > model.TimeMax)
				return "Edit_NoteTimeOutOfRange";

			if (info.Parent.Value?.GetLayerInfo() is { IsDecoration: true })
				return "Edit_CreateFailForDecoration";

			return null;
		}
	}
}