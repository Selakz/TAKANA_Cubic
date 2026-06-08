#nullable enable

using System.Collections.Generic;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using VContainer;

namespace MusicGame.ChartEditor.Select
{
	public class SelectColliderPluginSystem : HierarchySystem<SelectColliderPluginSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
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

		// Private
		[Inject] private readonly IGameAudioPlayer music = default!;
		[Inject, Key("stage")] private readonly IViewPool<ChartComponent> viewPool = default!;
		[Inject, Key("select-collider")] private readonly IViewPool<ChartComponent> pluginPool = default!;

		[Inject, Key("select-collider")]
		private readonly Dictionary<T3Flag, TextureAlignInfo> textureAlignInfos = default!;

		private readonly IClassifier<T3Flag> classifier = new T3ChartClassifier();

		// System Functions
		void Update()
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