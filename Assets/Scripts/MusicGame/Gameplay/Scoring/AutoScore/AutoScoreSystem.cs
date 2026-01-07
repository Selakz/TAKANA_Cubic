#nullable enable

using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Scoring.AutoScore
{
	public class AutoScoreSystem : T3System, ITickable
	{
		private const double MaxScore = 1_000_000;
		private readonly NotifiableProperty<double> score;
		private readonly NotifiableProperty<int> combo;
		private readonly NotifiableProperty<LevelInfo?> levelInfo;
		private readonly GameAudioPlayer music;

		public NotifiableProperty<NoteSortedDataset?> Dataset { get; } = new(null);

		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, () =>
			{
				Dataset.Value?.Dispose();
				Dataset.Value = null;
				var info = levelInfo.Value;
				if (info is not null) Dataset.Value = new(info.Chart);
			})
		};

		public AutoScoreSystem(
			[Key("score")] NotifiableProperty<double> score,
			[Key("combo")] NotifiableProperty<int> combo,
			NotifiableProperty<LevelInfo?> levelInfo,
			GameAudioPlayer music) : base(true)
		{
			this.score = score;
			this.combo = combo;
			this.levelInfo = levelInfo;
			this.music = music;
		}

		public void Tick()
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