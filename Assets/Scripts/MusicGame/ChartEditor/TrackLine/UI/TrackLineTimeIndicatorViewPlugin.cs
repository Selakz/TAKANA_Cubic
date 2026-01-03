#nullable enable

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
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLine.UI
{
	public class TrackLineTimeIndicatorViewPlugin : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private Color draggingColor;
		[SerializeField] private Color pastingColor;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(nodeDragPlugin.IsDragging, UpdateDraggingText),
			new PropertyRegistrar<bool>(isShiftPressing, UpdateDraggingText),
			new PropertyRegistrar<NodeCopyPastePlugin.PasteMode>(copyPastePlugin.Mode, () =>
			{
				var mode = copyPastePlugin.Mode.Value;
				var isPasting = mode != NodeCopyPastePlugin.PasteMode.None;
				if (isPasting)
				{
					var pasteText = I18NSystem.GetText(mode switch
					{
						NodeCopyPastePlugin.PasteMode.NormalPaste => "TrackLine_Pasting",
						NodeCopyPastePlugin.PasteMode.ExactPaste => "TrackLine_ExactPasting",
						_ => "Fallback"
					});
					timeIndicator.ColorModifier.Register(_ => pastingColor, 19);
					timeIndicator.TextModifier.Register(text => $"{pasteText} - {text}", 19);
				}
				else
				{
					timeIndicator.ColorModifier.Unregister(19);
					timeIndicator.TextModifier.Unregister(19);
				}
			}),
			new InputPressingRegistrar("General", "Shift", value => isShiftPressing.Value = value)
		};

		// Private
		private TimeIndicator timeIndicator = default!;
		private NodeDragPlugin nodeDragPlugin = default!;
		private NodeCopyPastePlugin copyPastePlugin = default!;

		private string draggingText = string.Empty;
		private readonly NotifiableProperty<bool> isShiftPressing = new(false);

		// Constructor
		[Inject]
		private void Construct(
			TimeIndicator timeIndicator,
			NodeDragPlugin nodeDragPlugin,
			NodeCopyPastePlugin copyPastePlugin)
		{
			this.timeIndicator = timeIndicator;
			this.nodeDragPlugin = nodeDragPlugin;
			this.copyPastePlugin = copyPastePlugin;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Defined Functions
		private void UpdateDraggingText()
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
	}
}