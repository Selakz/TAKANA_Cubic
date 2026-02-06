#nullable enable

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Event.UI;
using T3Framework.Static;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MusicGame.LevelSelect.UI
{
	public class LevelPanel : MonoBehaviour
	{
		// Serializable and Public
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
		public Button ChangeDifficultyButton { get; set; } = default!;

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
		private readonly int playfieldSceneIndex;
		private readonly DifficultyConfig difficultyConfig;
		private readonly GameObject loadingPanel;
		private readonly Texture defaultCoverTexture;

		private int currentDifficulty;

		public LevelPanelRegistrar(
			LevelPanel levelPanel,
			NotifiableProperty<RawLevelInfo<GameplayPreference>?> rawLevelInfo,
			NotifiableProperty<int> difficulty,
			LevelComponent<GameplayPreference> component,
			int playfieldSceneIndex,
			DifficultyConfig difficultyConfig,
			GameObject loadingPanel)
		{
			this.levelPanel = levelPanel;
			this.rawLevelInfo = rawLevelInfo;
			this.difficulty = difficulty;
			this.component = component;
			this.playfieldSceneIndex = playfieldSceneIndex;
			this.difficultyConfig = difficultyConfig;
			this.loadingPanel = loadingPanel;

			defaultCoverTexture = levelPanel.CoverImage.texture;
			currentDifficulty = this.component.Model.Preference.Value?.Difficulty ?? 3;
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
				_ => { rawLevelInfo.Value = component.Model; }),
			new PropertyRegistrar<RawLevelInfo<GameplayPreference>?>(rawLevelInfo, info =>
			{
				levelPanel.BgImage.color = ReferenceEquals(info, component.Model)
					? new(0.75f, 1, 1, 1)
					: Color.white;
			}),
			new ButtonRegistrar(levelPanel.StartGameButton, () => TryStartGame().Forget()),
			new ButtonRegistrar(levelPanel.ChangeDifficultyButton, () =>
			{
				if (component.Model.SongInfo.Value?.Difficulties is not { } difficulties) return;
				var numbers = difficulties.Keys.ToArray();
				if (numbers.Length == 0) return;
				var index = Array.IndexOf(numbers, currentDifficulty);
				LoadDifficulty(index < 0 ? numbers[0] : numbers[(index + 1) % numbers.Length]);
			})
		};

		private async UniTaskVoid TryStartGame()
		{
			loadingPanel.SetActive(true);
			var levelInfo = await component.Model.ToLevelInfo(currentDifficulty);
			if (levelInfo is null)
			{
				Debug.Log("failed to load level info");
				loadingPanel.SetActive(false);
				return;
			}

			GameplayLevelLoader.SetLevelInfo(levelInfo);
			SceneManager.LoadScene(playfieldSceneIndex);
		}

		private void LoadDifficulty(int difficulty)
		{
			if (component.Model.SongInfo.Value?.Difficulties is not { } difficulties ||
			    !difficulties.TryGetValue(difficulty, out var difficultyInfo))
			{
				levelPanel.DifficultyNameText.text = string.Empty;
				levelPanel.DifficultyValueText.text = string.Empty;
				return;
			}

			currentDifficulty = difficulty;
			if (difficultyConfig.Value.TryGetValue(difficulty, out var data))
			{
				levelPanel.DifficultyNameText.text = data.name;
				levelPanel.DifficultyNameText.color = data.color;
			}
			else levelPanel.DifficultyNameText.text = string.Empty;

			levelPanel.DifficultyValueText.text = difficultyInfo.LevelDisplay;

			if (ISingleton<PlayInfo>.Instance.GetPlayData(component.Model.SongInfo.Value.Id, difficulty) is { } info)
			{
				levelPanel.ScoreText.text = info.Score.ToString("0000000");
			}
			else
			{
				levelPanel.ScoreText.text = 0.ToString("0000000");
			}
		}

		protected override void Initialize()
		{
			var info = component.Model;
			levelPanel.CoverImage.texture = info.Cover.Value ?? defaultCoverTexture;
			levelPanel.SongNameText.text = info.SongInfo.Value?.Title.Value;
			if (info.Preference.Value is { } preference) currentDifficulty = preference.Difficulty;
			LoadDifficulty(currentDifficulty);
		}

		protected override void Deinitialize()
		{
			levelPanel.CoverImage.texture = defaultCoverTexture;
			levelPanel.SongNameText.text = string.Empty;
		}
	}
}