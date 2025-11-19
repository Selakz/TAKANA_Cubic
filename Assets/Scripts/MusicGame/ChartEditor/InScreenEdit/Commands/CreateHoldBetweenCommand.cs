#nullable enable

using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Level;
using MusicGame.ChartEditor.Select;
using MusicGame.Components.Notes;
using T3Framework.Runtime;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.Commands
{
	public class CreateHoldBetweenCommand : ISetInitCommand
	{
		public string Name => $"Create hold between {note1.Id} and {note2.Id}";

		private readonly BaseNote note1;
		private readonly BaseNote note2;
		private Hold? newHold;
		private T3Time oldHoldTimeEnd;

		private bool hasExecuted = false;

		public CreateHoldBetweenCommand(BaseNote note1, BaseNote note2)
		{
			if (note1.TimeJudge > note2.TimeJudge)
			{
				(note1, note2) = (note2, note1);
			}

			this.note1 = note1;
			this.note2 = note2;
		}

		public bool SetInit()
		{
			if (hasExecuted)
			{
				Debug.LogWarning("The command has already set init.");
				return false;
			}

			return note1.TimeJudge != note2.TimeJudge && note1.Parent.Id == note2.Parent.Id;
		}

		public void Do()
		{
			hasExecuted = true;

			if (note1 is Hold hold1)
			{
				oldHoldTimeEnd = hold1.TimeEnd;
				hold1.TimeEnd = note2.TimeJudge;
				IEditingChartManager.Instance.RemoveComponent(note2.Id);
				IEditingChartManager.Instance.UpdateComponent(hold1.Id);
			}
			else
			{
				if (note2 is Hold hold2)
				{
					oldHoldTimeEnd = hold2.TimeEnd;
					hold2.TimeEnd = hold2.TimeJudge;
					hold2.TimeJudge = note1.TimeJudge;
					IEditingChartManager.Instance.RemoveComponent(note1.Id);
					IEditingChartManager.Instance.UpdateComponent(hold2.Id);
				}
				else
				{
					newHold = new Hold(note1.TimeJudge, note2.TimeJudge, note1.Parent);
					IEditingChartManager.Instance.RemoveComponents(new[] { note1.Id, note2.Id });
					IEditingChartManager.Instance.AddComponent(newHold);
					if (ISelectManager.Instance != null)
					{
						ISelectManager.Instance.UnselectAll();
						ISelectManager.Instance.Select(newHold.Id);
					}
				}
			}
		}

		public void Undo()
		{
			if (note1 is Hold hold1)
			{
				hold1.TimeEnd = oldHoldTimeEnd;
				IEditingChartManager.Instance.UpdateComponent(hold1.Id);
				IEditingChartManager.Instance.AddComponent(note2);
			}
			else
			{
				if (note2 is Hold hold2)
				{
					hold2.TimeJudge = hold2.TimeEnd;
					hold2.TimeEnd = oldHoldTimeEnd;
					IEditingChartManager.Instance.UpdateComponent(hold2.Id);
					IEditingChartManager.Instance.AddComponent(note1);
				}
				else
				{
					if (newHold is not null) IEditingChartManager.Instance.RemoveComponent(newHold.Id);
					IEditingChartManager.Instance.AddComponents(new[] { note1, note2 });
					if (ISelectManager.Instance != null)
					{
						ISelectManager.Instance.UnselectAll();
						ISelectManager.Instance.SelectBundle(new[] { note1.Id, note2.Id });
					}
				}
			}
		}
	}
}