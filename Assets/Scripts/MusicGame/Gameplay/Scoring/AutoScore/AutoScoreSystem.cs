#nullable enable

using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Scoring.AutoScore
{
	public class AutoScoreSystem : HierarchySystem<AutoScoreSystem>
	{
		// Serializable and Public
		public NotifiableProperty<NoteSortedDataset?> Dataset { get; } = new(null);

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, () =>
			{
				Dataset.Value?.Dispose();
				Dataset.Value = null;
				var info = levelInfo.Value;
				if (info is not null) Dataset.Value = new(info.Chart, comboFactory);
			})
		};

		// Private
		[Inject, Key("score")] private readonly NotifiableProperty<double> score = default!;
		[Inject, Key("combo")] private readonly NotifiableProperty<int> combo = default!;
		[Inject] private readonly NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private readonly IGameAudioPlayer music = default!;
		[Inject] private readonly IComboFactory comboFactory = default!;

		// Static
		private const double MaxScore = 1_000_000;

		// System Functions
		void Update()
		{
			if (Dataset.Value is null || Dataset.Value.Times.Count == 0)
			{
				score.Value = 0;
				combo.Value = 0;
				return;
			}

			var index = Dataset.Value.GetLowerBoundIndex(music.ChartTime);
			score.Value = Mathf.RoundToInt((float)((double)index / Dataset.Value.Times.Count * MaxScore));
			combo.Value = index;
		}
	}
}