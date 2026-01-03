#nullable enable

using MusicGame.ChartEditor.Command;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Note;
using T3Framework.Runtime;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.Commands
{
	public class CreateHoldBetweenCommand : ISetInitCommand
	{
		public string Name => $"Create hold between {note1.Id} and {note2.Id}";

		public ChartComponent<Hold>? NewHold { get; private set; }

		private readonly ChartInfo? chart;
		private ChartComponent note1;
		private ChartComponent note2;
		private INote? model1;
		private INote? model2;
		private T3Time oldHoldTimeEnd;

		private bool hasExecuted = false;


		public CreateHoldBetweenCommand(ChartComponent note1, ChartComponent note2)
		{
			this.note1 = note1;
			this.note2 = note2;
			chart = note1.BelongingChart;
		}

		public bool SetInit()
		{
			if (hasExecuted)
			{
				Debug.LogWarning("The command has already set init.");
				return false;
			}

			if (note1.BelongingChart is null ||
			    note1.BelongingChart != note2.BelongingChart ||
			    !ReferenceEquals(note1.Parent, note2.Parent) ||
			    note1.Model is not INote note1Model ||
			    note2.Model is not INote note2Model ||
			    note1Model.TimeJudge == note2Model.TimeJudge) return false;

			model1 = note1Model;
			model2 = note2Model;
			if (model1.TimeJudge > model2.TimeJudge)
			{
				(note1, note2) = (note2, note1);
				(model1, model2) = (model2, model1);
			}

			return true;
		}

		public void Do()
		{
			hasExecuted = true;

			if (model1 is Hold hold1)
			{
				oldHoldTimeEnd = hold1.TimeEnd;
				note2.BelongingChart = null;
				note1.UpdateModel(_ => hold1.TimeEnd = model2!.TimeJudge);
			}
			else
			{
				if (model2 is Hold hold2)
				{
					oldHoldTimeEnd = hold2.TimeEnd;
					note1.BelongingChart = null;
					note2.UpdateModel(_ =>
					{
						hold2.TimeEnd = hold2.TimeJudge;
						hold2.TimeJudge = model1!.TimeJudge;
					});
				}
				else
				{
					var parent = note1.Parent;
					note1.BelongingChart = null;
					note2.BelongingChart = null;

					if (NewHold is not null) NewHold.BelongingChart = chart;
					else
					{
						var hold = new Hold(model1!.TimeJudge, model2!.TimeJudge);
						NewHold = chart!.AddComponentGeneric(hold);
					}

					NewHold.Parent = parent;
				}
			}
		}

		public void Undo()
		{
			if (model1 is Hold hold1)
			{
				note1.UpdateModel(_ => hold1.TimeEnd = oldHoldTimeEnd);
				note2.BelongingChart = chart;
				note2.Parent = note1.Parent;
			}
			else
			{
				if (model2 is Hold hold2)
				{
					note2.UpdateModel(_ =>
					{
						hold2.TimeJudge = hold2.TimeEnd;
						hold2.TimeEnd = oldHoldTimeEnd;
					});
					note1.BelongingChart = chart;
					note1.Parent = note2.Parent;
				}
				else
				{
					var parent = NewHold!.Parent;
					NewHold.BelongingChart = null;
					note1.BelongingChart = chart;
					note2.BelongingChart = chart;
					note1.Parent = parent;
					note2.Parent = parent;
				}
			}
		}
	}
}