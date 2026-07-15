#nullable enable

using System;
using System.Linq;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Event.UI;
using T3Framework.Runtime.Extensions;
using T3Framework.Static;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MusicGame.LevelSelect.UI
{
	public class LevelPanel : MonoBehaviour
	{
		[field: SerializeField]
		public PointerUpDownListener PointerListener { get; set; } = default!;

		[field: SerializeField]
		public Image BgImage { get; set; } = default!;

		[field: SerializeField]
		public RawImage CoverImage { get; set; } = default!;

		[field: SerializeField]
		public TMP_Text SongNameText { get; set; } = default!;

		[field: SerializeField]
		public Button StartGameButton { get; set; } = default!;

		[field: SerializeField]
		public TextMeshProUGUI DifficultyNameText { get; set; } = default!;

		[field: SerializeField]
		public TextMeshProUGUI DifficultyValueText { get; set; } = default!;

		[field: SerializeField]
		public TMP_Text ScoreText { get; set; } = default!;
	}

	public class LevelPanelRegistrar : CompositeRegistrar
	{
		private readonly LevelPanel levelPanel;
		private readonly NotifiableProperty<RawLevelInfo<GameplayPreference>?> rawLevelInfo;
		private readonly NotifiableProperty<int> difficulty;
		private readonly LevelComponent<GameplayPreference> component;
		private readonly DifficultyConfig difficultyConfig;
		private readonly PreEntryPanel preEntryPanel;
		private readonly Texture defaultCoverTexture;

		private int currentDifficulty;

		public LevelPanelRegistrar(
			LevelPanel levelPanel,
			NotifiableProperty<RawLevelInfo<GameplayPreference>?> rawLevelInfo,
			NotifiableProperty<int> difficulty,
			LevelComponent<GameplayPreference> component,
			DifficultyConfig difficultyConfig,
			PreEntryPanel preEntryPanel)
		{
			this.levelPanel = levelPanel;
			this.rawLevelInfo = rawLevelInfo;
			this.difficulty = difficulty;
			this.component = component;
			this.difficultyConfig = difficultyConfig;
			this.preEntryPanel = preEntryPanel;

			defaultCoverTexture = levelPanel.CoverImage.texture;
		}

		protected override IEventRegistrar[] InnerRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<EventHandler>(
				e => component.OnComponentUpdated += e,
				e => component.OnComponentUpdated -= e,
				(_, _) => Initialize()),
			CustomRegistrar.Generic<Action<PointerEventData>>(
				e => levelPanel.PointerListener.PointerClick += e,
				e => levelPanel.PointerListener.PointerClick -= e,
				_ =>
				{
					rawLevelInfo.Value = component.Model;
					difficulty.Value = currentDifficulty;
				}),
			new PropertyRegistrar<RawLevelInfo<GameplayPreference>?>(rawLevelInfo, info =>
			{
				levelPanel.BgImage.color = ReferenceEquals(info, component.Model)
					? new(0.75f, 1, 1, 1)
					: Color.white;
			}),
			new PropertyRegistrar<int>(difficulty, LoadDifficultyWithFallback),
			new ButtonRegistrar(levelPanel.StartGameButton, () =>
			{
				rawLevelInfo.Value = component.Model;
				preEntryPanel.Show();
			})
		};

		private void LoadDifficulty(int diff)
		{
			if (component.Model.SongInfo.Value?.Difficulties is not { } difficulties ||
			    !difficulties.TryGetValue(diff, out var difficultyInfo))
			{
				levelPanel.DifficultyNameText.text = string.Empty;
				levelPanel.DifficultyValueText.text = string.Empty;
				return;
			}

			if (difficultyConfig.Value.TryGetValue(diff, out var data))
			{
				levelPanel.DifficultyNameText.text = data.name;
				levelPanel.DifficultyNameText.color = data.color;
			}
			else levelPanel.DifficultyNameText.text = string.Empty;

			levelPanel.DifficultyValueText.text = difficultyInfo.LevelDisplay;
			levelPanel.ScoreText.text =
				ISingleton<PlayInfo>.Instance.GetPlayData(component.Model.SongInfo.Value.Id, diff) is { } info
					? info.Score.ToString("0000000")
					: 0.ToString("0000000");

			currentDifficulty = diff;
		}

		private void LoadDifficultyWithFallback(int diff)
		{
			if (component.Model.SongInfo.Value?.Difficulties is { } difficulties)
				LoadDifficulty(difficulties.ContainsKey(diff) ? diff : difficulties.Keys.DefaultIfEmpty(3).Max());
			else
				LoadDifficulty(difficulty);
		}

		protected override void Initialize()
		{
			var info = component.Model;
			levelPanel.CoverImage.LoadTextureCover(info.Cover.Value ?? defaultCoverTexture);
			levelPanel.SongNameText.text = info.SongInfo.Value?.Title.Value;
			LoadDifficultyWithFallback(difficulty);
		}

		protected override void Deinitialize()
		{
			levelPanel.CoverImage.LoadTextureCover(defaultCoverTexture);
			levelPanel.SongNameText.text = string.Empty;
		}
	}
}