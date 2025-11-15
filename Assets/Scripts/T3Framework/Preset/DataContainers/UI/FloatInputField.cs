#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using TMPro;
using UnityEngine;

namespace T3Framework.Preset.DataContainers.UI
{
	public class FloatInputField : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private NotifiableDataContainer<float> floatDataContainer = default!;
		[SerializeField] private string format = "0.000";
		[SerializeField] private float textDisplayScale = 1f;
		[SerializeField] private TMP_InputField inputField = default!;
		[SerializeField] private InputFieldRegistrar.RegisterTarget registerTarget = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<float>(floatDataContainer,
				(_, _) =>
				{
					inputField.SetTextWithoutNotify(
						(floatDataContainer.Property.Value * textDisplayScale).ToString(format));
				}),
			new InputFieldRegistrar(inputField, registerTarget, value =>
			{
				if (!float.TryParse(value, out var result) ||
				    Mathf.Approximately(result, floatDataContainer.Property.Value * textDisplayScale))
				{
					return;
				}

				floatDataContainer.Property.Value = result / textDisplayScale;
			})
		};
	}
}