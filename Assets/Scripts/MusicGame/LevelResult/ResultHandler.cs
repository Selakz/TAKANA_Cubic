#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;
using MusicGame.Gameplay.Level;
using MusicGame.LevelResult.UI;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.LevelResult
{
	public class ResultHandler : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private RawImage coverImage = default!;
		[SerializeField] private InfoHeaderView infoHeaderView = default!;
		[SerializeField] private TextMeshProUGUI scoreText = default!;
		[SerializeField] private List<JudgeDetailsView> judgeDetailsViews = default!;
		[SerializeField] private string scoreFormat = string.Empty;
		[SerializeField] private DifficultyConfig difficultyConfig = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<ResultInfo?>(resultInfo, () =>
			{
				if (resultInfo.Value is not { } info) return;

				if (info.LevelInfo is { } levelInfo)
				{
					ISingletonSetting<PlayInfo>.Instance.SetHighScore(
						info.LevelInfo.SongInfo.Id, info.LevelInfo.Difficulty, Mathf.RoundToInt((float)info.Score));
					ISingletonSetting<PlayInfo>.SaveInstance(); // TODO: Async
					UpdateSongInfo(levelInfo);
					coverImage.LoadTextureCover(levelInfo.Cover ?? defaultTexture);
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

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

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
		}

		private void UpdateScore(ResultInfo resultInfo)
		{
			scoreText.text = resultInfo.Score.ToString(scoreFormat);
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