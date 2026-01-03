#nullable enable

using System.ComponentModel;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.Utility.Setting
{
	public class BoolSettingItem : SingleValueSettingItem<bool>
	{
		// Serializable and Public
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
			base.InitializeSucceed();
			toggle.SetIsOnWithoutNotify(DisplayValue);
		}

		protected override void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
		{
			toggle.SetIsOnWithoutNotify(DisplayValue);
		}
	}
}