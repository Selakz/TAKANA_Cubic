#nullable enable

using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Setting;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;

namespace App
{
	public class AppSetting : ISingletonSetting<AppSetting>
	{
		public NotifiableProperty<bool> UseEnglish { get; set; } = new(false);

		public AppSetting()
		{
			UseEnglish.PropertyChanged += (_, _) =>
				I18NSystem.CurrentLanguageCode = UseEnglish.Value ? Language.English : Language.SimplifiedChinese;
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		public static void Trigger()
		{
			var setting = ISingleton<AppSetting>.Instance;
			I18NSystem.IsLoaded.PropertyChanged += (_, _) => I18NSystem.CurrentLanguageCode = setting.UseEnglish.Value
				? Language.English
				: Language.SimplifiedChinese;
			Debug.Log($"Initialize AppSetting. Use English: {setting.UseEnglish.Value}");
		}
	}
}