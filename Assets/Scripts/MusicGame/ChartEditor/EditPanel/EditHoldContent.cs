using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Message;
using TMPro;
using UnityEngine;

namespace MusicGame.ChartEditor.EditPanel
{
	/// <summary>
	/// For <see cref="EditingHold"/>
	/// </summary>
	public class EditHoldContent : EditNoteContent
	{
		// Serializable and Public
		[Header("Hold Content")] [SerializeField]
		private TMP_InputField timeEndInputField;

		public override EditingNote Model
		{
			get => model;
			set
			{
				base.Model = value;
				if (model is not EditingHold editingHold)
				{
					Debug.LogError($"EditHoldContent's Model is not {nameof(EditingHold)}");
					return;
				}

				timeEndInputField.text = editingHold.Hold.TimeEnd.ToString();
			}
		}

		// Event Handlers
		protected override void OnTimeJudgeInputFieldEndEdit(string content)
		{
			if (int.TryParse(content, out int newTimeJudge) &&
			    newTimeJudge < (Model as EditingHold)!.Hold.TimeEnd &&
			    newTimeJudge >= Model.Note.Parent.TimeInstantiate &&
			    newTimeJudge <= Model.Note.Parent.TimeEnd)
			{
				var oldTimeJudge = model.Note.TimeJudge;
				if (newTimeJudge == oldTimeJudge) return;
				CommandManager.Instance.Add(new UpdateComponentsCommand(new UpdateComponentArg(
					Model,
					model => (model as EditingNote)!.Note.TimeJudge = newTimeJudge,
					model => (model as EditingNote)!.Note.TimeJudge = oldTimeJudge)
				));
			}
			else
			{
				HeaderMessage.Show("ÐÞ¸ÄÊ§°Ü", HeaderMessage.MessageType.Warn);
				timeJudgeInputField.SetTextWithoutNotify(model.Note.TimeJudge.ToString());
			}
		}

		public void OnTimeEndInputFieldEndEdit(string timeEndInput)
		{
			EditingHold editingHold = (EditingHold)model;
			if (int.TryParse(timeEndInputField.text, out int newTimeEnd) &&
			    newTimeEnd > Model.Note.TimeJudge &&
			    newTimeEnd >= Model.Note.Parent.TimeInstantiate &&
			    newTimeEnd <= Model.Note.Parent.TimeEnd)
			{
				var oldTimeEnd = editingHold.Hold.TimeEnd;
				if (newTimeEnd == editingHold.Hold.TimeEnd) return;
				CommandManager.Instance.Add(new UpdateComponentsCommand(new UpdateComponentArg(
					Model,
					model => (model as EditingHold)!.Hold.TimeEnd = newTimeEnd,
					model => (model as EditingHold)!.Hold.TimeEnd = oldTimeEnd)
				));
			}
			else
			{
				HeaderMessage.Show("ÐÞ¸ÄÊ§°Ü", HeaderMessage.MessageType.Warn);
				timeEndInputField.SetTextWithoutNotify(editingHold.Hold.TimeEnd.ToString());
			}
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			timeEndInputField.onEndEdit.AddListener(OnTimeEndInputFieldEndEdit);
		}
	}
}