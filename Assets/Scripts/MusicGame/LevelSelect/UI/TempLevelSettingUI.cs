#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Preset.UICollection;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.LevelSelect.UI
{
	public class TempLevelSettingUI : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private FloatValueAdjuster speedAdjuster = default!;
		[SerializeField] private FloatValueAdjuster deviationAdjuster = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<float>(speedAdjuster.Property, () =>
			{
				var speed = speedAdjuster.Property.Value;
				ISingletonSetting<PlayfieldSetting>.Instance.Speed.Value = speed;
				ISingletonSetting<PlayfieldSetting>.SaveInstance();
			}),
			new PropertyRegistrar<float>(deviationAdjuster.Property, () =>
			{
				var deviation = deviationAdjuster.Property.Value;
				ISingletonSetting<PlayfieldSetting>.Instance.AudioDeviation.Value = deviation;
				ISingletonSetting<PlayfieldSetting>.SaveInstance();
			})
		};

		// System Functions
		protected override void OnEnable()
		{
			speedAdjuster.Property.Value = ISingletonSetting<PlayfieldSetting>.Instance.Speed;
			deviationAdjuster.Property.Value = ISingletonSetting<PlayfieldSetting>.Instance.AudioDeviation.Value.Second;
			base.OnEnable();
		}
	}
}