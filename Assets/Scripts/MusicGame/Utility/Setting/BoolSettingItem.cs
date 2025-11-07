#nullable enable

using System.ComponentModel;
using System.Reflection;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.Utility.Setting
{
	public class BoolSettingItem : SingleValueSettingItem<bool>
	{
		// Serializable and Public
		[SerializeField] private TMP_Text descriptionText = default!;
		[SerializeField] private Toggle toggle = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ToggleRegistrar(toggle, isOn =>
			{
				DisplayValue = isOn;
				Save();
			})
		};

		protected override void InitializeSucceed()
		{
			var descriptionAttribute = TargetPropertyInfo!.GetCustomAttribute<DescriptionAttribute>();
			descriptionText.text = descriptionAttribute is null ? string.Empty : descriptionAttribute.Description;

			toggle.SetIsOnWithoutNotify(DisplayValue);
		}

		protected override void InitializeFail()
		{
			descriptionText.text = $"Error fetching setting {FullClassName}.{PropertyName}";
		}

		protected override void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
		{
			toggle.SetIsOnWithoutNotify(DisplayValue);
		}
	}
}