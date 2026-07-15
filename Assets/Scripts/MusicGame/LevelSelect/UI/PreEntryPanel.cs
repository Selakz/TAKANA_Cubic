#nullable enable

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Level;
using MusicGame.Gameplay.Stage;
using T3Framework.Preset.Event;
using T3Framework.Preset.UICollection;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.LevelSelect.UI
{
	public class PreEntryPanel : HierarchySystem<PreEntryPanel>
	{
		// Serializable and Public
		[SerializeField] private GameObject panelRoot = default!;
		[SerializeField] private GameObject loadingPanel = default!;
		[SerializeField] private SongInfoPanel songInfoPanel = default!;
		[SerializeField] private InspectorDictionary<Toggle, GameplayStageSkinConfig> skinToggles = default!;
		[SerializeField] private FloatValueAdjuster speedAdjuster = default!;
		[SerializeField] private FloatValueAdjuster pitchAdjuster = default!;
		[SerializeField] private Toggle autoPlayToggle = default!;
		[SerializeField] private Button startButton = default!;
		[SerializeField] private Button closeButton = default!;
		[SerializeField] private int playfieldSceneIndex;
		[SerializeField] private string fallbackSkinName = string.Empty;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars
		{
			get
			{
				var registrars = new List<IEventRegistrar>
				{
					new PropertyRegistrar<RawLevelInfo<GameplayPreference>?>(rawLevelInfo, OnLevelSelectionChanged),
					new PropertyRegistrar<float>(speedAdjuster.Property, speed =>
					{
						ISingletonSetting<PlayfieldSetting>.Instance.Speed.Value = speed;
						ISingletonSetting<PlayfieldSetting>.SaveInstance();
					}),
					new PropertyRegistrar<float>(pitchAdjuster.Property, pitch =>
					{
						if (rawLevelInfo.Value?.Preference.Value is { } preference)
							preference.Pitch = pitch;
					}),
					new ToggleRegistrar(autoPlayToggle, isOn =>
					{
						if (rawLevelInfo.Value?.Preference.Value is { } preference)
							preference.IsAuto = isOn;
					}),
					new ButtonRegistrar(startButton, () => OnStartGame().Forget()),
					new ButtonRegistrar(closeButton, Hide)
				};

				foreach (var pair in skinToggles.Value)
				{
					var skinConfig = pair.Value;
					registrars.Add(new ToggleRegistrar(pair.Key, isOn =>
					{
						if (isOn && rawLevelInfo.Value?.Preference.Value is { } preference)
							preference.SkinNameLocalized = skinConfig.skinNameLocalized;
					}));
				}

				return registrars.ToArray();
			}
		}

		// Private
		[Inject] private NotifiableProperty<RawLevelInfo<GameplayPreference>?> rawLevelInfo = default!;
		[Inject] private NotifiableProperty<int> difficulty = default!;

		// Event Handlers
		private void OnLevelSelectionChanged(RawLevelInfo<GameplayPreference>? info)
		{
			if (info is null)
			{
				panelRoot.SetActive(false);
				return;
			}

			songInfoPanel.LoadCover(info.Cover.Value);
			songInfoPanel.LoadSongInfo(info.SongInfo.Value);
			songInfoPanel.LoadDifficulty(difficulty.Value);

			var preference = info.Preference.Value;
			if (preference is not null)
			{
				speedAdjuster.Property.Value = ISingletonSetting<PlayfieldSetting>.Instance.Speed;
				pitchAdjuster.Property.Value = preference.Pitch;
				autoPlayToggle.SetIsOnWithoutNotify(preference.IsAuto);

				foreach (var pair in skinToggles.Value)
				{
					var skinName = string.IsNullOrWhiteSpace(preference.SkinNameLocalized)
						? fallbackSkinName
						: preference.SkinNameLocalized;
					var isOn = pair.Value.skinNameLocalized == skinName;
					pair.Key.SetIsOnWithoutNotify(isOn);
				}
			}

			speedAdjuster.Property.Value = ISingletonSetting<PlayfieldSetting>.Instance.Speed;
		}

		// Defined Functions
		public void Show()
		{
			panelRoot.SetActive(true);
			OnLevelSelectionChanged(rawLevelInfo.Value);
		}

		public void Hide()
		{
			panelRoot.SetActive(false);
		}

		private async UniTaskVoid OnStartGame()
		{
			loadingPanel.SetActive(true);
			panelRoot.SetActive(false);

			var info = rawLevelInfo.Value;
			var diff = difficulty.Value;
			if (info is null) return;

			var projSetting = await ISetting<T3ProjSetting>.LoadAsync(info.LevelPath);
			var preference = info.Preference.Value;
			if (preference is not null)
			{
				var preferencePath = FileHelper.GetAbsolutePathFromRelative(
					info.LevelPath, projSetting.PreferenceFileName);
				await ISetting<GameplayPreference>.SaveAsync(preference, preferencePath);
			}

			var songInfo = info.SongInfo.Value;
			if (songInfo is not null && string.IsNullOrWhiteSpace(songInfo.Id))
			{
				songInfo.Id = Guid.NewGuid().ToString();
				var songInfoPath = FileHelper.GetAbsolutePathFromRelative(
					info.LevelPath, projSetting.SongInfoFileName);
				await ISetting<SongInfo>.SaveAsync(songInfo, songInfoPath);
			}

			var levelInfo = await info.ToLevelInfo(diff);
			if (levelInfo is null)
			{
				Debug.Log("failed to load level info");
				return;
			}

			GameplayLevelLoader.SetLevelInfo(levelInfo);
			SceneManager.LoadScene(playfieldSceneIndex);
		}

		// System Functions
		protected override void OnEnable()
		{
			speedAdjuster.Property.Value = ISingletonSetting<PlayfieldSetting>.Instance.Speed;
			base.OnEnable();
		}
	}
}