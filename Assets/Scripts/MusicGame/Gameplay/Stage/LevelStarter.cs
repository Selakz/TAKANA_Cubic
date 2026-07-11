#nullable enable

using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;

namespace MusicGame.Gameplay.Stage
{
	public class LevelStarter : HierarchySystem<LevelStarter>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, () =>
			{
				service.StopGenerate();
				music.Pause();
				if (levelInfo.Value is not { } info) return;

				T3Time offset = info.Chart.GetsOffsetInfo().Value;
				music.Load(info.Music!, offset);
				music.Play();
				service.StartGenerate(info.Chart, music);
			})
		};

		// Private
		[Inject] private NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private IGameAudioPlayer music = default!;
		[Inject] private IStageViewGenerateService service = default!;
	}
}