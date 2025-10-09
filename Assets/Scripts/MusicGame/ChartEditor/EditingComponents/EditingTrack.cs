using MusicGame.Components.Tracks;
using MusicGame.Gameplay.Level;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;

namespace MusicGame.ChartEditor.EditingComponents
{
	public class EditingTrack : EditingComponent
	{
		// Serializable and Public
		public new static string TypeMark => "e_track";

		public Track Track => Component as Track;

		// Private

		// Static

		// Defined Functions
		public EditingTrack(Track track) : base(track)
		{
		}

		public override bool Generate()
		{
			if (LevelManager.Instance.Music.ChartTime <=
			    Track.TimeEnd + ISingletonSetting<PlayfieldSetting>.Instance.TimeAfterEnd)
			{
				Track.Generate();
			}

			return true;
		}

		public override bool Destroy()
		{
			return Track.Destroy();
		}

		public static EditingTrack Deserialize(JToken token, object context)
		{
			if (token is not JContainer) return default;
			var track = Track.Deserialize(token, context);
			var editingTrack = new EditingTrack(track)
			{
				Properties = GetEditorConfig(token)
			};
			return editingTrack;
		}

		public EditingTrack Clone(T3Time timeStart, float xOffset)
		{
			return new EditingTrack(Track.Clone(timeStart, xOffset))
			{
				Properties = new JObject(Properties)
			};
		}
	}
}