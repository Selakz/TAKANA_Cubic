#nullable enable

using System.Collections.Generic;
using DLCore.Models.Note;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Judge;
using MusicGame.Models.Note;

namespace DLCore.Gameplay.Judge
{
	public class DLComboFactory : IComboFactory
	{
		public IEnumerable<IComboItem> CreateCombo(ChartComponent component)
		{
			if (component.Model is not IDLNote note || note.IsDummy()) yield break;
			switch (note)
			{
				case DLHit hit:
					yield return new DLHitCombo(component)
					{
						ExpectedTime = hit.TimeJudge,
						NeedTap = hit.Type == HitType.Tap,
						PlayHitSound = true,
						Color = hit.Color
					};
					break;
				case DLHold hold:
					yield return new DLHitCombo(component)
					{
						ExpectedTime = hold.TimeJudge,
						NeedTap = true,
						PlayHitSound = true,
						Color = hold.Color
					};
					yield return new DLHoldEndCombo(component)
					{
						ExpectedTime = hold.TimeEnd,
						PlayHitSound = false,
						Color = hold.Color
					};
					break;
			}
		}
	}
}
