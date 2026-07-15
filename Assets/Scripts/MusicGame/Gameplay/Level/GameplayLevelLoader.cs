#nullable enable

using System.Linq;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Stage;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Level
{
	public class GameplayLevelLoader : HierarchySystem<GameplayLevelLoader>
	{
		// Serializable and Public
		[SerializeField] private GameplayStageSkinConfig[] stageSkinConfigs = default!;
		[SerializeField] private GameObject judgeScoreModule = default!;
		[SerializeField] private GameObject autoScoreModule = default!;

		[SerializeField, Header("Voez Adaptation")]
		private GameplayStageSkinConfig voezConfig = default!;

		[SerializeField] private Color voezColor = default!;

		// Private
		[Inject] private NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private IGameAudioPlayer music = default!;
		[Inject] private NotifiableProperty<GameplayStageSkinConfig> stageSkinConfig = default!;

		private Color? originalTrackColor;

		// Static
		private static LevelInfo? toLoadLevelInfo;

		// Defined Functions
		public static void SetLevelInfo(LevelInfo levelInfo) => toLoadLevelInfo = levelInfo;

		// System Functions
		void Start()
		{
			if (toLoadLevelInfo is null) return;
			if (toLoadLevelInfo.Preference is GameplayPreference preference)
			{
				var skinConfig = stageSkinConfigs.FirstOrDefault(
					config => config.skinNameLocalized == preference.SkinNameLocalized);
				if (skinConfig is not null) stageSkinConfig.Value = skinConfig;

				// Hard-coded Voez adaptation
				if (skinConfig == voezConfig)
				{
					var trackColor = ISingleton<PlayfieldSetting>.Instance.TrackFaceDefaultColor.Value;
					if (Mathf.Approximately(trackColor.r, 0f) && Mathf.Approximately(trackColor.g, 0f) &&
					    Mathf.Approximately(trackColor.b, 0f))
					{
						originalTrackColor = trackColor;
						ISingleton<PlayfieldSetting>.Instance.TrackFaceDefaultColor.Value = voezColor with
						{
							a = Mathf.Min(voezColor.a, trackColor.a)
						};
					}
				}

				judgeScoreModule.SetActive(!preference.IsAuto);
				autoScoreModule.SetActive(preference.IsAuto);

				music.Pitch = preference.Pitch;
			}

			levelInfo.Value = toLoadLevelInfo;
			toLoadLevelInfo = null;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (originalTrackColor is not null)
			{
				ISingleton<PlayfieldSetting>.Instance.TrackFaceDefaultColor.Value = originalTrackColor.Value;
			}
		}
	}
}