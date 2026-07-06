#nullable enable

using MusicGame.Gameplay.Basic;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.SortingOrder
{
	public class SortingOrderSystem : HierarchySystem<SortingOrderSystem>
	{
		// Serializable and Public
		[SerializeField] private SpriteSortingOrderMapConfig sortingOrderMapConfig = default!;
		[SerializeField] private SequenceConfig sortingOrderConfig = default!;
		[SerializeField] private SequencePriority sortingOrderPriority = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(viewPool, handler => new CustomRegistrar(
				() =>
				{
					var component = viewPool[handler]!;
					var presenter = handler.Script<IT3ModelViewPresenter>();

					foreach (var (type, textureMap) in sortingOrderMapConfig.Value)
					{
						if (!T3ChartClassifier.Instance.IsOfType(component, type)) continue;
						foreach (var (sortingOrderName, textureName) in textureMap)
						{
							var sortingOrder = sortingOrderConfig.Priorities[sortingOrderName];
							if (presenter.Textures.TryGetValue(textureName, out var texture))
							{
								texture.SortingOrderModifier.Register(_ => sortingOrder, sortingOrderPriority);
							}
						}
					}
				},
				() =>
				{
					var component = viewPool[handler]!;
					var presenter = handler.Script<IT3ModelViewPresenter>();

					foreach (var (type, textureMap) in sortingOrderMapConfig.Value)
					{
						if (!T3ChartClassifier.Instance.IsOfType(component, type)) continue;
						foreach (var textureName in textureMap.Values)
						{
							if (presenter.Textures.TryGetValue(textureName, out var texture))
							{
								texture.SortingOrderModifier.Unregister(sortingOrderPriority);
							}
						}
					}
				}))
		};

		// Private
		[Inject, Key("stage")] private IViewPool<ChartComponent> viewPool = default!;
	}
}