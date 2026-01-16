#nullable enable

using T3Framework.Preset.DataContainers;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace T3Framework.Preset.UICollection
{
	public class FloatValueAdjuster : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private FloatDataContainer valueContainer = default!;
		[SerializeField] private Button minusButton = default!;
		[SerializeField] private Button plusButton = default!;
		[SerializeField] private TMP_InputField valueInputField = default!;

		[SerializeField] private string format = default!;
		[SerializeField] private float jumpValue;

		public NotifiableProperty<float> Property => valueContainer.Property;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<float>(valueContainer.Property, UpdateUI),
			new ButtonRegistrar(minusButton, () => valueContainer.Property.Value -= jumpValue),
			new ButtonRegistrar(plusButton, () => valueContainer.Property.Value += jumpValue),
			new InputFieldRegistrar(valueInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
				content =>
				{
					if (float.TryParse(content, out var value) &&
					    !Mathf.Approximately(value, valueContainer.Property.Value))
					{
						valueContainer.Property.Value = value;
					}
					else
					{
						valueInputField.SetTextWithoutNotify(valueContainer.Property.Value.ToString(format));
					}
				})
		};

		// Defined Functions
		public void UpdateUI()
		{
			valueInputField.SetTextWithoutNotify(valueContainer.Property.Value.ToString(format));
		}
	}
}