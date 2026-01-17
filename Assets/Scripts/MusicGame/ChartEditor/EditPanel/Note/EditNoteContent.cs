using System;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Note;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Event.UI;
using T3Framework.Runtime.Log;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.EditPanel.Note
{
	/// <summary>
	/// For notes with only one time property, otherwise may cause mistakes.
	/// </summary>
	public class EditNoteContent : MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public EditNoteConfig Config { get; set; } = default!;

		[field: SerializeField]
		public EditNameTitle NameTitle { get; set; } = default!;

		[field: SerializeField]
		public Image NoteTypeImage { get; set; } = default!;

		[field: SerializeField]
		public Image IsNoteDummyImage { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField TimeJudgeInputField { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField TimeEndInputField { get; set; } = default!;

		[field: SerializeField]
		public Button UnselectButton { get; set; } = default!;

		[field: SerializeField]
		public Button DeleteButton { get; set; } = default!;

		[field: SerializeField]
		public Button JumpToTrackButton { get; set; } = default!;

		[field: SerializeField]
		public PointerUpDownListener PointerListener { get; set; } = default!;
	}

	public class NoteContentRegistrar : IEventRegistrar
	{
		private readonly EditNoteContent noteContent;
		private readonly ChartComponent component;
		private readonly IEventRegistrar[] registrars;

		public NoteContentRegistrar(
			EditNoteContent noteContent,
			ChartComponent component,
			ChartSelectDataset dataset,
			IGameAudioPlayer audioPlayer,
			ChartEditSystem chartEditSystem)
		{
			this.noteContent = noteContent;
			this.component = component;

			registrars = new IEventRegistrar[]
			{
				CustomRegistrar.Generic<EventHandler>(
					e => component.OnComponentUpdated += e,
					e => component.OnComponentUpdated -= e,
					(_, _) => UpdateUI()),
				new ButtonRegistrar(noteContent.UnselectButton, () => { dataset.Remove(component); }),
				new ButtonRegistrar(noteContent.DeleteButton, () =>
				{
					var command = new DeleteComponentCommand(component);
					CommandManager.Instance.Add(command);
				}),
				new ButtonRegistrar(noteContent.JumpToTrackButton, () =>
				{
					dataset.Clear();
					if (component.Parent is not null) dataset.Add(component.Parent);
					audioPlayer.ChartTime = ((INote)component.Model).TimeJudge;
				}),
				new InputFieldRegistrar(noteContent.TimeJudgeInputField,
					InputFieldRegistrar.RegisterTarget.OnEndEdit,
					content =>
					{
						var note = (INote)component.Model;
						if (int.TryParse(content, out int newTimeJudge))
						{
							if (newTimeJudge == note.TimeJudge) return;
							var distance = newTimeJudge - note.TimeJudge;
							var command = chartEditSystem.NudgeNoteJudge(component, distance);
							if (command is not null) CommandManager.Instance.Add(command);
						}
						else
						{
							T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
							noteContent.TimeJudgeInputField.SetTextWithoutNotify(note.TimeJudge.ToString());
						}
					}),
				new InputFieldRegistrar(noteContent.TimeEndInputField,
					InputFieldRegistrar.RegisterTarget.OnEndEdit,
					content =>
					{
						if (component.Model is not Hold model) return;
						if (int.TryParse(content, out int newTimeEnd))
						{
							if (newTimeEnd == model.TimeEnd) return;
							var distance = newTimeEnd - model.TimeEnd;
							var command = chartEditSystem.NudgeHoldEnd(component, distance) ?? EmptyCommand.Instance;
							CommandManager.Instance.Add(command);
						}
						else
						{
							T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
							noteContent.TimeEndInputField.SetTextWithoutNotify(model.TimeEnd.ToString());
						}
					}),
				CustomRegistrar.Generic<Action<PointerEventData>>(
					e => noteContent.PointerListener.PointerClick += e,
					e => noteContent.PointerListener.PointerClick -= e,
					eventData =>
					{
						if (eventData.pointerCurrentRaycast.gameObject == noteContent.IsNoteDummyImage.gameObject)
						{
							var note = (INote)component.Model;
							var isDummy = !note.IsDummy();
							CommandManager.Instance.Add(new UpdateComponentCommand(component,
								_ => note.SetDummy(isDummy), _ => note.SetDummy(!isDummy)));
							noteContent.IsNoteDummyImage.sprite = isDummy
								? noteContent.Config.DummyNoteImage
								: noteContent.Config.RealNoteImage;
						}
					}),
				new NameTitleRegistrar(noteContent.NameTitle, component)
			};
		}

		private void UpdateUI()
		{
			var note = (INote)component.Model;
			noteContent.TimeJudgeInputField.text = note.TimeJudge.ToString();
			noteContent.TimeEndInputField.interactable = component.Model is Hold;
			noteContent.TimeEndInputField.text = component.Model is Hold hold ? hold.TimeEnd.ToString() : "-";

			noteContent.IsNoteDummyImage.sprite = note.IsDummy()
				? noteContent.Config.DummyNoteImage
				: noteContent.Config.RealNoteImage;

			noteContent.NoteTypeImage.sprite = component.Model switch
			{
				Hit { Type: HitType.Tap } => noteContent.Config.TapImage,
				Hit { Type: HitType.Slide } => noteContent.Config.SlideImage,
				Hold => noteContent.Config.HoldImage,
				_ => null
			};
			noteContent.transform.localScale = Vector3.one;
		}

		public void Register()
		{
			UpdateUI();
			foreach (var registrar in registrars) registrar.Register();
		}

		public void Unregister()
		{
			foreach (var registrar in registrars) registrar.Unregister();
		}
	}
}