#nullable enable

using MusicGame.ChartEditor.Command;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Note;
using UnityEngine;

namespace MusicGame.ChartEditor.Draft.Commands
{
	public class CreateDraftHoldBetweenCommand : ISetInitCommand
	{
		public string Name => $"Create draft hold between {note1.Id} and {note2.Id}";

		public ChartComponent<DraftHold>? NewHold { get; private set; }

		private readonly ChartInfo? chart;
		private ChartComponent note1;
		private ChartComponent note2;
		private ISolitaryNote? model1;
		private ISolitaryNote? model2;

		private bool hasExecuted = false;

		public CreateDraftHoldBetweenCommand(ChartComponent note1, ChartComponent note2)
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
			    note1.Model is not ISolitaryNote note1Model ||
			    note2.Model is not ISolitaryNote note2Model ||
			    !T3ChartClassifier.Instance.IsOfType(note1, T3Flag.Draft) ||
			    !T3ChartClassifier.Instance.IsOfType(note2, T3Flag.Draft) ||
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

			var parent = note1.Parent;
			note1.BelongingChart = null;
			note2.BelongingChart = null;

			if (NewHold is not null) NewHold.BelongingChart = chart;
			else
			{
				var hold = new DraftHold(model1!.TimeJudge, model2!.TimeJudge, model1.Position, model1.Width);
				NewHold = chart!.AddComponentGeneric(hold);
			}

			NewHold.Parent = parent;
		}

		public void Undo()
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