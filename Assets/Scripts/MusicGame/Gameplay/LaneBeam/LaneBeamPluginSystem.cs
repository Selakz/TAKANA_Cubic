#nullable enable

using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using VContainer;

namespace MusicGame.Gameplay.LaneBeam
{
	public class LaneBeamPluginSystem : T3System
	{
		private readonly SubViewPool<ChartComponent, T3Flag> viewPool;
		private readonly IViewPool<ChartComponent> pluginPool;

		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
		{
			new ViewPoolPluginRegistrar<ChartComponent>(viewPool, pluginPool, "lane-beam")
		};

		public LaneBeamPluginSystem(
			[Key("stage")] IViewPool<ChartComponent> viewPool,
			[Key("lane-beam")] IViewPool<ChartComponent> pluginPool) : base(true)
		{
			this.pluginPool = pluginPool;
			pluginPool.IsGetActive = false;
			this.viewPool = new SubViewPool<ChartComponent, T3Flag>(viewPool, new T3ChartClassifier(), T3Flag.Track);
		}
	}
}