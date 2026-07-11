#nullable enable

using System.Linq;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Performance;
using MusicGame.Gameplay.Stage;
using MusicGame.Models;
using MusicGame.Models.Track;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
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
			new ViewPoolPluginRegistrar<ChartComponent>(viewPool, pluginPool, "lane-beam",
				component =>
				{
					var handler = pluginPool[component]!;
					handler.transform.localPosition = Vector3.zero;
				}),
			new PropertyRegistrar<GameplayStageSkinConfig>(skinConfig, config =>
			{
				if (pluginPool is not ViewPool<ChartComponent> pool) return;
				pool.Prefab = config.laneBeamPrefab;
				var plugins = pool.ToList();
				pool.Clear();
				foreach (var component in plugins) pool.Add(component);
			})
		};

		// Private
		[Inject] private NotifiableProperty<GameplayStageSkinConfig> skinConfig = default!;
		[Inject] private IGameAudioPlayer music = default!;
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

		// System Functions
		void Update()
		{
			foreach (var component in pluginPool)
			{
				if (component.Model is not ITrack track) continue;
				var plugin = pluginPool[component]!.Script<LaneBeamPlugin>();
				plugin.Width = track.Movement.GetWidth(music.ChartTime);
				if (skinConfig.Value.trackBehaviour is TrackBehaviour.Falling)
				{
					plugin.transform.localPosition = new(track.Movement.GetPos(music.ChartTime), 0);
				}
			}
		}
	}
}