#nullable enable

using System;
using MusicGame.Gameplay.Basic;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.SortingOrder
{
	public class SortingOrderSystem : IInitializable, IDisposable
	{
		private readonly T3Flag targetType;
		private readonly SpriteSortingOrderMapConfig sortingOrderMapConfig;
		private readonly int sortingOrderPriority;
		private readonly SequenceConfig sortingOrderConfig;
		private readonly SubViewPool<ChartComponent, T3Flag> orderPool;

		public SortingOrderSystem(
			T3Flag targetType,
			SpriteSortingOrderMapConfig sortingOrderMapConfig,
			int sortingOrderPriority,
			[Key("sortingOrder")] SequenceConfig sortingOrderConfig,
			[Key("stage")] IViewPool<ChartComponent> viewPool)
		{
			this.targetType = targetType;
			this.sortingOrderMapConfig = sortingOrderMapConfig;
			this.sortingOrderPriority = sortingOrderPriority;
			this.sortingOrderConfig = sortingOrderConfig;
			orderPool = new SubViewPool<ChartComponent, T3Flag>(viewPool, new T3ChartClassifier(), targetType);
		}

		public void Initialize()
		{
			orderPool.OnDataAdded += OnDataAdded;
			orderPool.BeforeDataRemoved += BeforeDataRemoved;
		}

		private void OnDataAdded(ChartComponent component)
		{
			var presenter = orderPool[component]!.Script<IT3ModelViewPresenter>();
			var configTextureMap = sortingOrderMapConfig.Value[targetType];
			foreach (var pair in configTextureMap)
			{
				var sortingOrder = sortingOrderConfig.Priorities[pair.Key];
				var texture = presenter.Textures[pair.Value];
				texture.SortingOrderModifier.Register(_ => sortingOrder, sortingOrderPriority);
			}
		}

		private void BeforeDataRemoved(ChartComponent component)
		{
			var presenter = orderPool[component]!.Script<IT3ModelViewPresenter>();
			var configTextureMap = sortingOrderMapConfig.Value[targetType];
			foreach (var textureName in configTextureMap.Values)
			{
				var texture = presenter.Textures[textureName];
				texture.SortingOrderModifier.Unregister(sortingOrderPriority);
			}
		}

		public void Dispose()
		{
			orderPool.OnDataAdded -= OnDataAdded;
			orderPool.BeforeDataRemoved -= BeforeDataRemoved;
			orderPool.Dispose();
		}
	}
}