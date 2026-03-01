#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using UnityEngine;
using UnityEngine.UI;

namespace App.Utility.UI
{
	public class SwitchLanguageButton : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Button button = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(button, () =>
			{
				ISingletonSetting<AppSetting>.Instance.UseEnglish.Value =
					!ISingletonSetting<AppSetting>.Instance.UseEnglish.Value;
				ISingletonSetting<AppSetting>.SaveInstance();
			})
		};
	}
}