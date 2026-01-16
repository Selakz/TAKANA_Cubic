#nullable enable

using System.Collections.Generic;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Note;
using MusicGame.Models.Track;

namespace MusicGame.Gameplay.Judge.T3
{
	public class T3ComboFactory : IComboFactory
	{
		public IEnumerable<IComboItem> CreateCombo(ChartComponent component)
		{
			if (component.Model is not INote note || note.IsDummy()) yield break;

			GetEdges(component, out var leftEdge, out var rightEdge);
			switch (note)
			{
				case Hit hit:
					yield return new HitCombo(component)
					{
						ExpectedTime = hit.TimeJudge,
						LeftEdge = leftEdge,
						RightEdge = rightEdge,
						NeedTap = hit.Type == HitType.Tap
					};
					break;
				case Hold hold:
					yield return new HitCombo(component)
					{
						ExpectedTime = hold.TimeJudge,
						LeftEdge = leftEdge,
						RightEdge = rightEdge,
						NeedTap = true
					};
					yield return new HoldEndCombo(component)
					{
						ExpectedTime = hold.TimeEnd,
					};
					break;
				default:
					break;
			}
		}

		public static void GetEdges(ChartComponent component, out float leftEdge, out float rightEdge)
		{
			if (component.Parent?.Model is not ITrack track)
			{
				leftEdge = 100;
				rightEdge = 101;
				return;
			}

			const float extraRange = 0.1f;
			// TODO: May calculate an interval before the time judge to add up range
			var left = track.Movement.GetLeftPos(component.Model.TimeMin);
			var right = track.Movement.GetRightPos(component.Model.TimeMin);
			if (left > right) (left, right) = (right, left);
			leftEdge = left - extraRange;
			rightEdge = right + extraRange;
		}
	}
}