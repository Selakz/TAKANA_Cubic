#nullable enable

using System.Linq;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.InScreenEdit.CopyPaste;
using MusicGame.ChartEditor.InScreenEdit.UI;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine.UI
{
	public class TrackLineTimeIndicatorViewPlugin : HierarchySystem<TrackLineTimeIndicatorViewPlugin>
	{
		// Serializable and Public
		[SerializeField] private Color draggingColor;
		[SerializeField] private Color pastingColor;

		[SerializeField] private SequencePriorities copyPasteModules = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(nodeDragPlugin.IsDragging, UpdateDraggingView),
			new PropertyRegistrar<bool>(isShiftPressing, UpdateDraggingView),
			new PropertyRegistrar<int>(moduleInfo.CurrentModule, UpdateCopyPasteView),
			new PropertyRegistrar<PasteMode>(pasteMode, UpdateCopyPasteView),
			new InputPressingRegistrar("General", "Shift", value => isShiftPressing.Value = value)
		};

		// Private
		[Inject] private readonly ModuleInfo moduleInfo = default!;
		[Inject] private readonly TimeIndicator timeIndicator = default!;
		[Inject] private readonly NodeDragPlugin nodeDragPlugin = default!;
		[Inject] private readonly NotifiableProperty<PasteMode> pasteMode = default!;

		private string draggingText = string.Empty;
		private readonly NotifiableProperty<bool> isShiftPressing = new(false);

		// Defined Functions
		private void UpdateDraggingView()
		{
			var isDragging = nodeDragPlugin.IsDragging.Value;
			if (isDragging)
			{
				draggingText = isShiftPressing
					? I18NSystem.GetText("TrackLine_NodeSplitDragging")
					: I18NSystem.GetText("TrackLine_NodeDragging");
				timeIndicator.ColorModifier.Register(_ => draggingColor, 20);
				timeIndicator.TextModifier.Register(text => $"{draggingText} - {text}", 20);
			}
			else
			{
				timeIndicator.ColorModifier.Unregister(20);
				timeIndicator.TextModifier.Unregister(20);
			}
		}

		private void UpdateCopyPasteView()
		{
			if (copyPasteModules.Values.All(id => id != moduleInfo.CurrentModule))
			{
				timeIndicator.ColorModifier.Unregister(19);
				timeIndicator.TextModifier.Unregister(19);
				return;
			}

			var pasteText = I18NSystem.GetText(pasteMode.Value switch
			{
				PasteMode.NormalPaste => "TrackLine_Pasting",
				PasteMode.ExactPaste => "TrackLine_ExactPasting",
				_ => "Fallback"
			});
			timeIndicator.ColorModifier.Register(_ => pastingColor, 19);
			timeIndicator.TextModifier.Register(text => $"{pasteText} - {text}", 19);
		}
	}
}