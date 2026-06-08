#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Note;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using TMPro;
using UnityEngine;

namespace MusicGame.ChartEditor.EditPanel.Note
{
	public class EditDraftNoteContent : EditNoteContent
	{
		[field: SerializeField]
		public TMP_InputField PositionInputField { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField WidthInputField { get; set; } = default!;
	}

	public class DraftNoteContentRegistrar : NoteContentRegistrar
	{
		private readonly IEventRegistrar[] registrars;

		protected override IEnumerable<IEventRegistrar> Registrars => base.Registrars.Concat(registrars);

		public DraftNoteContentRegistrar(
			EditDraftNoteContent noteContent,
			ChartComponent component,
			ChartSelectDataset dataset,
			IGameAudioPlayer audioPlayer,
			ChartEditSystem chartEditSystem,
			CommandManager commandManager) :
			base(noteContent, component, dataset, audioPlayer, chartEditSystem, commandManager)
		{
			registrars = new IEventRegistrar[]
			{
				new InputFieldRegistrar(noteContent.PositionInputField,
					InputFieldRegistrar.RegisterTarget.OnEndEdit,
					content =>
					{
						var note = (ISolitaryNote)component.Model;
						if (float.TryParse(content, out float newPosition))
						{
							if (Mathf.Approximately(newPosition, note.Position)) return;
							var positionBefore = note.Position;
							commandManager.Add(new UpdateComponentCommand(component,
								_ => note.Position = newPosition,
								_ => note.Position = positionBefore));
						}
						else
						{
							T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
							noteContent.PositionInputField.SetTextWithoutNotify(note.Position.ToString("0.00"));
						}
					}),
				new InputFieldRegistrar(noteContent.WidthInputField,
					InputFieldRegistrar.RegisterTarget.OnEndEdit,
					content =>
					{
						var note = (ISolitaryNote)component.Model;
						if (float.TryParse(content, out float newWidth))
						{
							if (Mathf.Approximately(newWidth, note.Width)) return;
							var widthBefore = note.Width;
							commandManager.Add(new UpdateComponentCommand(component,
								_ => note.Width = newWidth,
								_ => note.Width = widthBefore));
						}
						else
						{
							T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
							noteContent.WidthInputField.SetTextWithoutNotify(note.Width.ToString("0.00"));
						}
					})
			};
		}

		protected override void UpdateUI()
		{
			base.UpdateUI();
			var note = (ISolitaryNote)component.Model;
			((EditDraftNoteContent)noteContent).PositionInputField.SetTextWithoutNotify(note.Position.ToString("0.00"));
			((EditDraftNoteContent)noteContent).WidthInputField.SetTextWithoutNotify(note.Width.ToString("0.00"));
		}
	}
}