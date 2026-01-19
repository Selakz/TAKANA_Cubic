#nullable enable

using System.Collections.Generic;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Select
{
	public class SelectColliderPluginSystem : T3System, ITickable
	{
		// Private
		private readonly IClassifier<T3Flag> classifier;
		private readonly IGameAudioPlayer music;
		private readonly IViewPool<ChartComponent> viewPool;
		private readonly IViewPool<ChartComponent> pluginPool;
		private readonly Dictionary<T3Flag, TextureAlignInfo> textureAlignInfos;

		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
		{
			new ViewPoolPluginRegistrar<ChartComponent>(viewPool, pluginPool, "select-collider",
				data =>
				{
					var classType = classifier.Classify(data);
					foreach (var pair in textureAlignInfos)
					{
						if (classifier.IsSubType(pair.Key, classType))
						{
							var handler = pluginPool[data];
							var plugin = handler!.Script<SelectColliderPlugin>();
							plugin.gameObject.layer = pair.Value.colliderLayer;
							plugin.StartAligning(pair.Value);
							plugin.ColliderModifier.EnabledModifier.Register(
								_ => data.Model.TimeMax > music.ChartTime, 1);
						}
					}
				},
				data => !data.Model.IsEditorOnly())
		};

		// Defined Functions
		public SelectColliderPluginSystem(
			IGameAudioPlayer music,
			[Key("stage")] IViewPool<ChartComponent> viewPool,
			[Key("select-collider")] IViewPool<ChartComponent> pluginPool,
			[Key("select-collider")] Dictionary<T3Flag, TextureAlignInfo> textureAlignInfos) : base(true)
		{
			classifier = new T3ChartClassifier();
			this.music = music;
			this.viewPool = viewPool;
			this.pluginPool = pluginPool;
			this.textureAlignInfos = textureAlignInfos;
		}

		// System Functions
		public void Tick()
		{
			foreach (var component in pluginPool)
			{
				if (pluginPool[component]?.Script<SelectColliderPlugin>() is { } plugin)
				{
					plugin.ColliderModifier.EnabledModifier.Update();
				}
			}
		}
	}
}