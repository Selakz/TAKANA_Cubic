#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;

namespace T3Framework.Preset.DataContainers.UI
{
	public class BoolToggle : T3MonoBehaviour
	{
		[SerializeField] private NotifiableDataContainer<bool> boolDataContainer = default!;
		[SerializeField] private Toggle toggle = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ToggleRegistrar(toggle, value => boolDataContainer.Property.Value = value),
			new DataContainerRegistrar<bool>
				(boolDataContainer, (_, _) => toggle.SetIsOnWithoutNotify(boolDataContainer.Property.Value))
		};
	}
}