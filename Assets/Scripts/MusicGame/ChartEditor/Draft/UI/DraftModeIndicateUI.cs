#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.ChartEditor.Draft.UI
{
	public class DraftModeIndicateUI : HierarchySystem<DraftModeIndicateUI>
	{
		// Serializable and Public
		[SerializeField] private Sprite draftSprite = default!;
		[SerializeField] private Image[] draftImages = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(draftContainer.IsInDraftMode, value =>
			{
				foreach (var image in draftImages) image.sprite = value ? draftSprite : null;
				if (value) T3Logger.Log("Notice", "Edit_Draft_EnterDraftMode", T3LogType.Info);
			})
		};

		// Private
		[Inject] private DraftContainer draftContainer = default!;
	}
}