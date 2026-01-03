#nullable enable

using System.Linq;
using MusicGame.ChartEditor.InScreenEdit.CopyPaste;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit.UI
{
	public class TimeIndicatorViewPlugin : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private Color legalColor;
		[SerializeField] private Color illegalColor;
		[SerializeField] private Color pastingColor;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(noteDragPlugin.IsDragging, () =>
			{
				var isDragging = noteDragPlugin.IsDragging.Value;
				shouldUpdateDraggingView = isDragging;
				if (isDragging)
				{
					var draggingText = I18NSystem.GetText("Edit_Dragging");
					timeIndicator.ColorModifier.Register(color => draggingColor ?? color, 2);
					timeIndicator.TextModifier.Register(text => $"{draggingText} - {text}", 2);
				}
				else
				{
					timeIndicator.ColorModifier.Unregister(2);
					timeIndicator.TextModifier.Unregister(2);
				}
			}),
			new PropertyRegistrar<CopyPastePlugin.PasteMode>(copyPastePlugin.Mode, () =>
			{
				var mode = copyPastePlugin.Mode.Value;
				var isPasting = mode != CopyPastePlugin.PasteMode.None;
				if (isPasting)
				{
					var pasteText = I18NSystem.GetText(mode switch
					{
						CopyPastePlugin.PasteMode.NormalPaste => "Edit_CopyPaste_Pasting",
						CopyPastePlugin.PasteMode.ExactPaste => "Edit_CopyPaste_ExactPasting",
						_ => "Fallback"
					});
					timeIndicator.ColorModifier.Register(_ => pastingColor, 1);
					timeIndicator.TextModifier.Register(text => $"{pasteText} - {text}", 1);
				}
				else
				{
					timeIndicator.ColorModifier.Unregister(1);
					timeIndicator.TextModifier.Unregister(1);
				}
			})
		};

		// Private
		private StageMouseTimeRetriever timeRetriever = default!;
		private TimeIndicator timeIndicator = default!;
		private NoteDragPlugin noteDragPlugin = default!;
		private CopyPastePlugin copyPastePlugin = default!;

		private Color? draggingColor = new(0.4f, 0.9f, 0.5f);
		private bool shouldUpdateDraggingView = false;

		// Defined Functions
		[Inject]
		private void Construct(
			StageMouseTimeRetriever timeRetriever,
			TimeIndicator timeIndicator,
			NoteDragPlugin noteDragPlugin,
			CopyPastePlugin copyPastePlugin)
		{
			this.timeRetriever = timeRetriever;
			this.timeIndicator = timeIndicator;
			this.noteDragPlugin = noteDragPlugin;
			this.copyPastePlugin = copyPastePlugin;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// System Functions
		void Update()
		{
			if (shouldUpdateDraggingView)
			{
				if (!timeRetriever.GetMouseTimeStart(out var mouseTime)) return;
				var beginTime = noteDragPlugin.BeginTime;
				var distance = mouseTime - beginTime;
				if (Mathf.Abs(distance) < ISingleton<InScreenEditSetting>.Instance.TimeDragThreshold.Value)
				{
					draggingColor = null;
					timeIndicator.ColorModifier.Update();
					return;
				}

				if (noteDragPlugin.DraggingNotes.Any(c => !c.IsWithinParentRange(distance)))
				{
					if (draggingColor != illegalColor)
					{
						draggingColor = illegalColor;
						timeIndicator.ColorModifier.Update();
					}
				}
				else
				{
					if (draggingColor != legalColor)
					{
						draggingColor = legalColor;
						timeIndicator.ColorModifier.Update();
					}
				}
			}
		}
	}
}