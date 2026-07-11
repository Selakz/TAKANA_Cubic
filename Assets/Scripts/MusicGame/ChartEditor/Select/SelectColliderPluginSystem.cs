#nullable enable

using System.Linq;
using Cysharp.Threading.Tasks;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Stage;
using MusicGame.Models;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;

namespace MusicGame.ChartEditor.Select
{
	public class SelectColliderPluginSystem : HierarchySystem<SelectColliderPluginSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolPluginRegistrar<ChartComponent>(viewPool, pluginPool, "select-collider",
				null,
				data =>
				{
					if (skinConfig.Value is not EditorStageSkinConfig config) return false;
					return !data.Model.IsEditorOnly() &&
					       config.textureAlignInfos.Value.Keys.Any(key => classifier.IsOfType(data, key));
				}),
			new ViewPoolLifetimeRegistrar<ChartComponent>(pluginPool, handler => new CustomRegistrar(
				() =>
				{
					if (skinConfig.Value is not EditorStageSkinConfig config) return;
					var data = pluginPool[handler]!;
					foreach (var pair in config.textureAlignInfos.Value)
					{
						if (classifier.IsOfType(data, pair.Key))
						{
							var plugin = handler!.Script<SelectColliderPlugin>();
							plugin.gameObject.layer = pair.Value.colliderLayer;
							plugin.ColliderModifier.EnabledModifier.Register(
								_ => data.Model.TimeMax > music.ChartTime, 0);
							UniTask.DelayFrame(1).ContinueWith(() => plugin.StartAligning(pair.Value));
							return;
						}
					}
				},
				() => handler.Script<SelectColliderPlugin>().ColliderModifier.EnabledModifier.Unregister(0)))
		};

		// Private
		[Inject] private readonly IGameAudioPlayer music = default!;
		[Inject] private IStageViewGenerateService service = default!;
		[Inject] private NotifiableProperty<GameplayStageSkinConfig> skinConfig = default!;
		[Inject, Key("stage")] private readonly IViewPool<ChartComponent> viewPool = default!;
		[Inject, Key("select-collider")] private readonly IViewPool<ChartComponent> pluginPool = default!;

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