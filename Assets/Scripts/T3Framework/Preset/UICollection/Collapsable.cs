#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;

namespace T3Framework.Preset.UICollection
{
	public class Collapsable : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private NotifiableDataContainer<bool> boolDataContainer = default!;
		[SerializeField] private Image backgroundImage = default!;
		[SerializeField] private Image arrowImage = default!;
		[SerializeField] private GameObject content = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<bool, NotifiableDataContainer<bool>>
			(boolDataContainer, (_, _) =>
			{
				var value = boolDataContainer.Property.Value;
				backgroundImage.enabled = value;
				content.gameObject.SetActive(value);

				var scale = arrowImage.rectTransform.localScale;
				arrowImage.rectTransform.localScale = new(scale.x, value ? -1 : 1, scale.z);
			})
		};
	}
}