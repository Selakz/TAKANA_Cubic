#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.Draft
{
	public class DraftModeAligner : HierarchySystem<DraftModeAligner>
	{
		// Serializable and Public
		[SerializeField] private GameObject[] enableObjects = default!;
		[SerializeField] private GameObject[] disableObjects = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(draftContainer.IsInDraftMode,
				value =>
				{
					foreach (var obj in enableObjects) obj.SetActive(value);
					foreach (var obj in disableObjects) obj.SetActive(!value);
				})
		};

		// Private
		[Inject] private DraftContainer draftContainer = default!;
	}
}