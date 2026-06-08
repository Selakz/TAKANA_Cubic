#nullable enable

using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using VContainer;

namespace MusicGame.ChartEditor.Draft
{
	public class DraftSelectManageSystem : HierarchySystem<DraftSelectManageSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(draftContainer.IsInDraftMode, selectDataset.Clear),
			// Notes are not allowed to be selected when in draft mode
			new ViewPoolLifetimeRegistrar<ChartComponent>(pluginPool, handler => new CustomRegistrar(
				() =>
				{
					var component = pluginPool[handler]!;
					if (T3ChartClassifier.Instance.IsOfType(component, T3Flag.Live | T3Flag.Note))
					{
						var plugin = handler.Script<SelectColliderPlugin>();
						plugin.ColliderModifier.EnabledModifier.Register(
							_ => !draftContainer.IsInDraftMode, 5);
					}
					else if (T3ChartClassifier.Instance.IsOfType(component, T3Flag.Draft | T3Flag.Note))
					{
						var plugin = handler.Script<SelectColliderPlugin>();
						plugin.ColliderModifier.EnabledModifier.Register(
							_ => draftContainer.IsInDraftMode, 5);
					}
				},
				() =>
				{
					var component = pluginPool[handler]!;
					if (T3ChartClassifier.Instance.IsOfType(component, T3Flag.Live | T3Flag.Note) ||
					    T3ChartClassifier.Instance.IsOfType(component, T3Flag.Draft | T3Flag.Note))
					{
						var plugin = handler.Script<SelectColliderPlugin>();
						plugin.ColliderModifier.EnabledModifier.Unregister(5);
					}
				}), true)
		};

		// Private
		[Inject] private DraftContainer draftContainer = default!;
		[Inject] private ChartSelectDataset selectDataset = default!;
		[Inject, Key("select-collider")] private readonly IViewPool<ChartComponent> pluginPool = default!;
	}
}