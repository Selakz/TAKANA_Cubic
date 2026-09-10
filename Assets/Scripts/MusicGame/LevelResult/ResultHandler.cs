#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;
using MusicGame.Gameplay.Level;
using MusicGame.LevelResult.UI;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.LevelResult
{
	public class ResultHandler : HierarchySystem<ResultHandler>
	{
		// Serializable and Public
		[SerializeField] private RawImage coverImage = default!;
		[SerializeField] private InfoHeaderView infoHeaderView = default!;
		[SerializeField] private TextMeshProUGUI scoreText = default!;
		[SerializeField] private TextMeshProUGUI highScoreText = default!;
		[SerializeField] private TextMeshProUGUI deltaScoreText = default!;
		[SerializeField] private List<JudgeDetailsView> judgeDetailsViews = default!;
		[SerializeField] private string scoreFormat = string.Empty;
		[SerializeField] private string deltaFormat = "+0;-0;0";
		[SerializeField] private DifficultyConfig difficultyConfig = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<ResultInfo?>(resultInfo, () =>
			{
				if (resultInfo.Value is not { } info) return;

				if (info.LevelInfo is { Preference: GameplayPreference preference } levelInfo)
				{
					UpdateSongInfo(levelInfo);
					coverImage.LoadTextureCover(levelInfo.Cover ?? defaultTexture);
					var songId = info.LevelInfo.SongInfo.Id;
					var difficulty = info.LevelInfo.Difficulty;
					var thisScore = Mathf.RoundToInt((float)info.Score);
					var oldScore = ISingletonSetting<PlayInfo>.Instance.GetPlayData(songId, difficulty)?.Score;
					var isRecorded = !preference.IsAuto && Mathf.Approximately(preference.Pitch, 1);
					if (isRecorded)
					{
						ISingletonSetting<PlayInfo>.Instance.SetHighScore(songId, difficulty, thisScore);
						ISingletonSetting<PlayInfo>.SaveInstance(); // TODO: Async
					}

					UpdateHighScore(thisScore, oldScore, isRecorded);
				}

				UpdateScore(info);
				UpdateJudgeDetails(info);
			})
		};

		// Private
		private NotifiableProperty<ResultInfo?> resultInfo = default!;
		private Texture defaultTexture = default!;

		// Constructor
		[Inject]
		private void Construct(NotifiableProperty<ResultInfo?> resultInfo)
		{
			this.resultInfo = resultInfo;
			defaultTexture = coverImage.texture;
		}

		// Defined Functions
		private void UpdateSongInfo(LevelInfo levelInfo)
		{
			infoHeaderView.SongNameText.text = levelInfo.SongInfo.Title.Value;
			if (difficultyConfig.Value.TryGetValue(levelInfo.Difficulty, out var data))
			{
				infoHeaderView.DifficultyNameText.text = data.name;
				infoHeaderView.DifficultyNameText.color = data.color;
			}
			else
			{
				infoHeaderView.DifficultyNameText.text = string.Empty;
				infoHeaderView.DifficultyNameText.color = Color.white;
			}

			infoHeaderView.DifficultyValueText.text =
				levelInfo.SongInfo.Difficulties.TryGetValue(levelInfo.Difficulty, out var difficulty)
					? difficulty.LevelDisplay
					: "00";

			if (levelInfo.Preference is GameplayPreference preference)
			{
				infoHeaderView.AutoPlayIndicator.SetActive(preference.IsAuto);
				infoHeaderView.PitchText.gameObject.SetActive(!Mathf.Approximately(preference.Pitch, 1));
				infoHeaderView.PitchText.text = $"x{preference.Pitch:0.00}";
			}
		}

		private void UpdateScore(ResultInfo resultInfo)
		{
			scoreText.text = resultInfo.Score.ToString(scoreFormat);
		}

		private void UpdateHighScore(int thisScore, int? oldScore, bool isRecorded)
		{
			highScoreText.text = (oldScore ?? 0).ToString(scoreFormat);
			deltaScoreText.text = isRecorded ? (thisScore - (oldScore ?? 0)).ToString(deltaFormat) : string.Empty;
		}

		private void UpdateJudgeDetails(ResultInfo resultInfo)
		{
			Dictionary<T3JudgeResult, int> judgeCounts = new();
			foreach (var item in resultInfo.JudgeItems ?? Array.Empty<IJudgeItem>())
			{
				if (item is IT3JudgeItem judgeItem)
				{
					judgeCounts.TryAdd(judgeItem.JudgeResult, 0);
					judgeCounts[judgeItem.JudgeResult]++;
				}
			}

			foreach (var details in judgeDetailsViews)
			{
				details.SetValue(judgeCounts);
			}
		}
	}
}