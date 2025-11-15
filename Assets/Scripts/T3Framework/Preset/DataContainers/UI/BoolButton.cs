#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;

namespace T3Framework.Preset.DataContainers.UI
{
	public class BoolButton : T3MonoBehaviour
	{
		[SerializeField] private NotifiableDataContainer<bool> boolDataContainer = default!;
		[SerializeField] private Button button = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(button, () => boolDataContainer.Property.Value = !boolDataContainer.Property.Value)
		};
	}
}