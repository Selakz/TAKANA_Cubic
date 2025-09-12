using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Message;
using MusicGame.ChartEditor.Select;
using MusicGame.Components.Notes;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.MVC;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.EditPanel
{
	/// <summary>
	/// For notes with only one time property, otherwise may cause mistakes.
	/// </summary>
	public class EditNoteContent : MonoBehaviour, IController<EditingNote>, IEditComponentContent, IPointerClickHandler
	{
		// Serializable and Public
		[SerializeField] private EditNoteConfig config;
		[SerializeField] private EditNameTitle nameTitle;
		[SerializeField] private Image noteTypeImage;
		[SerializeField] private Image isNoteDummyImage;
		[SerializeField] protected TMP_InputField timeJudgeInputField;
		[SerializeField] private Button unselectButton;
		[SerializeField] private Button deleteButton;
		[SerializeField] private Button jumpToTrackButton;

		public IModel GenericModel => Model;

		public virtual EditingNote Model
		{
			get => model;
			set
			{
				model = value;
				nameTitle.Model = model;
				timeJudgeInputField.text = model.Note.TimeJudge.ToString();
				isNoteDummyImage.sprite =
					model.Note.Properties.Get("isDummy", false) ? config.DummyNoteImage : config.RealNoteImage;
				noteTypeImage.sprite = model.Note switch
				{
					Tap => config.TapImage,
					Slide => config.SlideImage,
					Hold => config.HoldImage,
					_ => null
				};
				transform.localScale = Vector3.one;
			}
		}

		EditingComponent IEditComponentContent.Model
		{
			get => Model;
			set
			{
				if (value is not EditingNote note)
				{
					Debug.LogError($"EditNoteContent.Model only receives {nameof(EditingNote)}");
					return;
				}

				Model = note;
			}
		}

		public GameObject Object => gameObject;

		// Private
		protected EditingNote model;

		// Static

		// Defined Functions
		public void Init(EditingNote model)
		{
			Model = model;
		}

		public void Destroy()
		{
			// Released to Pool
			Object.SetActive(false);
		}

		// Event Handlers
		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.pointerCurrentRaycast.gameObject == isNoteDummyImage.gameObject)
			{
				var isDummy = !Model.Note.Properties.Get("isDummy", false);
				CommandManager.Instance.Add(new UpdateComponentsCommand(new UpdateComponentArg(
					Model,
					model => (model as EditingNote)!.Note.Properties["isDummy"] = isDummy,
					model => (model as EditingNote)!.Note.Properties["isDummy"] = !isDummy)
				));
				isNoteDummyImage.sprite = isDummy ? config.DummyNoteImage : config.RealNoteImage;
			}
		}

		protected void JumpToTrack()
		{
			ISelectManager.Instance.UnselectAll();
			ISelectManager.Instance.Select(Model.Parent.Id);
			EventManager.Instance.Invoke("Level_OnReset", Model.Note.TimeJudge);
		}

		private void OnUnselectButtonPressed()
		{
			ISelectManager.Instance.Unselect(Model.Id);
		}

		private void OnDeleteButtonPressed()
		{
			var command = new DeleteComponentsCommand(Model);
			command.OnRedo += () =>
			{
				ISelectManager.Instance.UnselectAll();
				ISelectManager.Instance.Select(Model.Id);
			};
			CommandManager.Instance.Add(command);
		}

		protected virtual void OnTimeJudgeInputFieldEndEdit(string content)
		{
			if (int.TryParse(content, out int newTimeJudge) &&
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

		// System Functions
		protected virtual void Awake()
		{
			unselectButton.onClick.AddListener(OnUnselectButtonPressed);
			deleteButton.onClick.AddListener(OnDeleteButtonPressed);
			jumpToTrackButton.onClick.AddListener(JumpToTrack);
			timeJudgeInputField.onEndEdit.AddListener(OnTimeJudgeInputFieldEndEdit);
		}
	}
}