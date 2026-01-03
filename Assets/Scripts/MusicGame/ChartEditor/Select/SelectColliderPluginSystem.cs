#nullable enable

using System.Collections.Generic;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using VContainer;

namespace MusicGame.ChartEditor.Select
{
	public class SelectColliderPluginSystem : T3System
	{
		// Private
		private readonly IClassifier<T3Flag> classifier;
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
						}
					}
				},
				data => !data.Model.IsEditorOnly())
		};

		// Defined Functions
		public SelectColliderPluginSystem(
			[Key("stage")] IViewPool<ChartComponent> viewPool,
			[Key("select-collider")] IViewPool<ChartComponent> pluginPool,
			[Key("select-collider")] Dictionary<T3Flag, TextureAlignInfo> textureAlignInfos) : base(true)
		{
			classifier = new T3ChartClassifier();
			this.viewPool = viewPool;
			this.pluginPool = pluginPool;
			this.textureAlignInfos = textureAlignInfos;
		}
	}
}