#nullable enable

using MusicGame.Models;
using MusicGame.Models.Note;
using MusicGame.Models.Note.Combo;

namespace MusicGame.Gameplay.Scoring.AutoScore
{
	public static class T3ComboGetter
	{
		public static IComboInfo GetComboInfo(INote note, IComboInfo? previousInfo)
		{
			return note switch
			{
				not null when note.IsDummy() || note.IsEditorOnly() => EmptyCombo.Instance,
				Hit hit => previousInfo is SingleCombo sc
					? sc.Set(hit.TimeJudge)
					: new SingleCombo(hit.TimeJudge),
				Hold hold => previousInfo is DoubleCombo dc
					? dc.Set(hold.TimeJudge, hold.TimeEnd)
					: new DoubleCombo(hold.TimeJudge, hold.TimeEnd),
				_ => EmptyCombo.Instance
			};
		}
	}
}