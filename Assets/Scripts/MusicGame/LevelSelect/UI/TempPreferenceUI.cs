#nullable enable

using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using VContainer;

namespace MusicGame.LevelSelect.UI
{
	public class TempPreferenceUI : HierarchySystem<TempPreferenceUI>
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField inputField = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<RawLevelInfo<GameplayPreference>?>(rawLevelInfo, info =>
			{
				inputField.interactable = info is not null;
				if (info?.Preference.Value is { } preference)
				{
					inputField.SetTextWithoutNotify(preference.SongDeviation.Second.ToString("0.000"));
				}
			}),
			new InputFieldRegistrar(inputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, value =>
			{
				if (rawLevelInfo.Value is not { } info || info.Preference.Value is not { } preference) return;
				if (float.TryParse(value, out var deviation) &&
				    !Mathf.Approximately(deviation, preference.SongDeviation.Second))
				{
					preference.SongDeviation = deviation;
					var projSetting = ISetting<T3ProjSetting>.Load(info.LevelPath);
					var preferencePath = FileHelper.GetAbsolutePathFromRelative(
						info.LevelPath, projSetting.PreferenceFileName);
					ISetting<GameplayPreference>.Save(preference, preferencePath);
				}

				inputField.SetTextWithoutNotify(preference.SongDeviation.Second.ToString("0.000"));
			})
		};

		// Private
		private NotifiableProperty<RawLevelInfo<GameplayPreference>?> rawLevelInfo = default!;

		// Constructor
		[Inject]
		private void Construct(NotifiableProperty<RawLevelInfo<GameplayPreference>?> rawLevelInfo)
		{
			this.rawLevelInfo = rawLevelInfo;
		}
	}
}