#nullable enable

using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Performance;
using MusicGame.Gameplay.Stage;
using MusicGame.Models;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using VContainer;

namespace MusicGame.Gameplay.LaneBeam
{
	public class LaneBeamPluginSystem : HierarchySystem<LaneBeamPluginSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(ISingleton<PerformanceSetting>.Instance.EnableLaneBeam,
				value => IsEnabled = value)
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolPluginRegistrar<ChartComponent>(viewPool, pluginPool, "lane-beam")
		};

		// Private
		private SubViewPool<ChartComponent, T3Flag> viewPool = default!;
		private IViewPool<ChartComponent> pluginPool = default!;

		// Constructor
		[Inject]
		private void Construct(
			[Key("stage")] IViewPool<ChartComponent> viewPool,
			[Key("lane-beam")] IViewPool<ChartComponent> pluginPool)
		{
			this.pluginPool = pluginPool;
			pluginPool.IsGetActive = false;
			this.viewPool = new SubViewPool<ChartComponent, T3Flag>(viewPool, new T3ChartClassifier(), T3Flag.Track);
		}
	}
}