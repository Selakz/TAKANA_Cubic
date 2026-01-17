#nullable enable

using System;
using Cysharp.Threading.Tasks;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Level;
using MusicGame.LevelResult;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Threading;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Scoring.JudgeScore
{
	public class LevelResultJumper : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private int delayTimeMilli;
		[SerializeField] private int resultSceneIndex;
		[SerializeField] private Button quickJumpButton = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<Action>(
				e => comboStorage.OnComboReset += e,
				e => comboStorage.OnComboReset -= e,
				() =>
				{
					rcts.CancelAndReset();
					quickJumpButton.gameObject.SetActive(false);
				}),
			new ButtonRegistrar(quickJumpButton, JumpToResultScene)
		};

		// Private
		private IGameAudioPlayer music = default!;
		private ComboStorage comboStorage = default!;
		private JudgeStorage judgeStorage = default!;
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private NotifiableProperty<double> score = default!;
		private NotifiableProperty<int> combo = default!;
		private NotifiableProperty<int> maxCombo = default!;

		private bool startCountdown = false;
		private readonly ReusableCancellationTokenSource rcts = new();

		// Constructor
		[Inject]
		private void Construct(
			IGameAudioPlayer music,
			ComboStorage comboStorage,
			JudgeStorage judgeStorage,
			NotifiableProperty<LevelInfo?> levelInfo,
			[Key("score")] NotifiableProperty<double> score,
			[Key("combo")] NotifiableProperty<int> combo,
			[Key("maxCombo")] NotifiableProperty<int> maxCombo)
		{
			this.music = music;
			this.comboStorage = comboStorage;
			this.judgeStorage = judgeStorage;
			this.levelInfo = levelInfo;
			this.score = score;
			this.combo = combo;
			this.maxCombo = maxCombo;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Defined Functions
		private void JumpToResultScene()
		{
			rcts.Cancel();
			ResultInfo resultInfo = new()
			{
				LevelInfo = levelInfo.Value,
				Score = score.Value,
				Combo = combo.Value,
				MaxCombo = maxCombo.Value,
				ComboItems = comboStorage.Combos,
				JudgeItems = judgeStorage.JudgeItems.Values
			};
			ResultLoader.SetResultInfo(resultInfo);
			SceneManager.LoadScene(resultSceneIndex);
		}

		// System Functions
		void Update()
		{
			if (comboStorage.Combos.Count == 0) return;

			if (music.ChartTime >= comboStorage.Combos[^1].ExpectedTime)
			{
				quickJumpButton.gameObject.SetActive(true);
			}

			if (music.AudioTime >= music.AudioLength && !startCountdown)
			{
				startCountdown = true;
				rcts.CancelAndReset();
				UniTask.Delay(delayTimeMilli, cancellationToken: rcts.Token).ContinueWith(JumpToResultScene);
			}
		}
	}
}