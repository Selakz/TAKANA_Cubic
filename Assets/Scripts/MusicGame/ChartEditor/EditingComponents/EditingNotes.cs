using MusicGame.Components.Notes;
using MusicGame.Gameplay.Level;
using Newtonsoft.Json.Linq;

namespace MusicGame.ChartEditor.EditingComponents
{
	public class EditingTap : EditingNote
	{
		public new static string TypeMark => "e_tap";

		public EditingTap(Tap tap) : base(tap)
		{
		}

		public override bool Generate()
		{
			if (LevelManager.Instance.Music.ChartTime <= Note.TimeJudge)
			{
				return Note.Generate();
			}

			return true;
		}

		public static EditingTap Deserialize(JToken token, object context)
		{
			if (token is not JContainer) return default;
			var tap = Tap.Deserialize(token, context);
			var editingTap = new EditingTap(tap)
			{
				Properties = GetEditorConfig(token)
			};
			return editingTap;
		}
	}

	public class EditingSlide : EditingNote
	{
		public new static string TypeMark => "e_slide";

		public EditingSlide(Slide tap) : base(tap)
		{
		}

		public override bool Generate()
		{
			if (LevelManager.Instance.Music.ChartTime <= Note.TimeJudge)
			{
				return Note.Generate();
			}

			return true;
		}

		public static EditingSlide Deserialize(JToken token, object context)
		{
			if (token is not JContainer) return default;
			var slide = Slide.Deserialize(token, context);
			var editingSlide = new EditingSlide(slide)
			{
				Properties = GetEditorConfig(token)
			};
			return editingSlide;
		}
	}

	public class EditingHold : EditingNote
	{
		public new static string TypeMark => "e_hold";

		public Hold Hold => Note as Hold;

		public EditingHold(Hold hold) : base(hold)
		{
		}

		public override bool Generate()
		{
			if (LevelManager.Instance.Music.ChartTime <= Note.TimeJudge)
			{
				return Note.Generate();
			}

			return true;
		}

		public static EditingHold Deserialize(JToken token, object context)
		{
			if (token is not JContainer) return default;
			var hold = Hold.Deserialize(token, context);
			var editingHold = new EditingHold(hold)
			{
				Properties = GetEditorConfig(token)
			};
			return editingHold;
		}
	}
}