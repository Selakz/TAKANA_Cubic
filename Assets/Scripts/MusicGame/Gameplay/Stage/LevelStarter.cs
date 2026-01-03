#nullable enable

using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Static.Event;

namespace MusicGame.Gameplay.Stage
{
	public class LevelStarter : T3System
	{
		private readonly NotifiableProperty<LevelInfo?> levelInfo;
		private readonly GameAudioPlayer music;
		private readonly StageManager stageManager;


		// Event Registrars
		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, () =>
			{
				stageManager.StopGenerate();
				music.Pause();
				if (levelInfo.Value is not { } info) return;

				T3Time offset = info.Chart.GetsOffsetInfo().Value;
				music.Load(info.Music!, offset);
				music.Play();
				stageManager.StartGenerate(info.Chart, music, new T3GeneralViewTimeCalculator());
			})
		};

		public LevelStarter(
			NotifiableProperty<LevelInfo?> levelInfo,
			GameAudioPlayer music,
			StageManager stageManager) : base(true)
		{
			this.levelInfo = levelInfo;
			this.music = music;
			this.stageManager = stageManager;
		}
	}
}