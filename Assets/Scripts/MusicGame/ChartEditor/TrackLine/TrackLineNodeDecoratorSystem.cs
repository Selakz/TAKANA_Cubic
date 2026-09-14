#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Stage;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine
{
	/// <summary>
	/// 轨道线世界结点的延迟实例化。
	/// 基于TimeWindowViewGenerator + IGameAudioPlayer.ChartTime，根据当前时间窗口动态创建/销毁Node视图。
	/// </summary>
	public class TrackLineNodeDecoratorSystem : HierarchySystem<TrackLineNodeDecoratorSystem>
	{
		// Serializable and Public
		[SerializeField] private string edgeNodeNamePrefix = default!;
		[SerializeField] private string directNodeNamePrefix = default!;
		[SerializeField] private int windowSizeMs = 10000;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			// Edge
			new DatasetRegistrar<EdgeNodeComponent>(edgeDataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataAdded,
				component => edgeGenerator.Add(component)),
			new DatasetRegistrar<EdgeNodeComponent>(edgeDataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataRemoved,
				component =>
				{
					edgeGenerator.Remove(component);
					pendingEdgeNodes.Remove(component);
				}),
			new DatasetRegistrar<EdgeNodeComponent>(edgeDataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataUpdated,
				component => edgeGenerator.Update(component)),

			// Direct
			new DatasetRegistrar<DirectNodeComponent>(directDataset,
				DatasetRegistrar<DirectNodeComponent>.RegisterTarget.DataAdded,
				component => directGenerator.Add(component)),
			new DatasetRegistrar<DirectNodeComponent>(directDataset,
				DatasetRegistrar<DirectNodeComponent>.RegisterTarget.DataRemoved,
				component =>
				{
					directGenerator.Remove(component);
					pendingDirectNodes.Remove(component);
				}),
			new DatasetRegistrar<DirectNodeComponent>(directDataset,
				DatasetRegistrar<DirectNodeComponent>.RegisterTarget.DataUpdated,
				component => directGenerator.Update(component)),

			new ViewPoolLifetimeRegistrar<EdgePMLComponent>(edgeDecoratorPool, OnEdgePMLViewGet, true),
			new ViewPoolLifetimeRegistrar<DirectPMLComponent>(directDecoratorPool, OnDirectPMLViewGet, true),
		};

		// Private
		[Inject] private EdgeNodeDataset edgeDataset = default!;
		[Inject] private IViewPool<EdgePMLComponent> edgeDecoratorPool = default!;
		[Inject] private IViewPool<EdgeNodeComponent> edgeViewPool = default!;
		[Inject] private DirectNodeDataset directDataset = default!;
		[Inject] private IViewPool<DirectPMLComponent> directDecoratorPool = default!;
		[Inject] private IViewPool<DirectNodeComponent> directViewPool = default!;
		[Inject] private IGameAudioPlayer music = default!;

		// TimeWindowViewGenerator
		private TimeWindowViewGenerator<EdgeNodeComponent> edgeGenerator = default!;
		private TimeWindowViewGenerator<DirectNodeComponent> directGenerator = default!;
		private readonly HashSet<EdgeNodeComponent> pendingEdgeNodes = new();
		private readonly HashSet<DirectNodeComponent> pendingDirectNodes = new();

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			var windowTime = new T3Time(windowSizeMs);
			edgeGenerator = new(new EdgeNodeTimeCalculator(windowTime));
			directGenerator = new(new DirectNodeTimeCalculator(windowTime));
		}

		void Update()
		{
			var time = music.ChartTime;
			RefreshEdge(time);
			RefreshDirect(time);
		}

		private void RefreshEdge(T3Time time)
		{
			edgeGenerator.RefreshTime(time, out var toInstantiate, out var toDestroy);
			foreach (var node in toInstantiate)
			{
				if (edgeDataset[node] is not { } moveList ||
				    edgeDecoratorPool[moveList] is not { } decorator)
				{
					pendingEdgeNodes.Add(node);
					continue;
				}

				CreateEdgeNodeView(node, decorator);
			}

			foreach (var node in toDestroy)
			{
				if (edgeDataset[node] is not { } moveList ||
				    edgeDecoratorPool[moveList] is not { } decorator) continue;
				DestroyEdgeNodeView(node, decorator);
			}
		}

		private void CreateEdgeNodeView(EdgeNodeComponent node, PrefabHandler decorator)
		{
			var nodeName = $"{edgeNodeNamePrefix}{node.Locator.Time.Milli}";
			if (edgeViewPool.Add(node)) decorator.AddPlugin(nodeName, edgeViewPool[node]!);
		}

		private void DestroyEdgeNodeView(EdgeNodeComponent node, PrefabHandler decorator)
		{
			var nodeHandler = edgeViewPool[node];
			if (edgeViewPool.Remove(node)) decorator.RemovePlugin(nodeHandler!, edgeDecoratorPool.DefaultTransform);
		}

		private IEventRegistrar OnEdgePMLViewGet(PrefabHandler handler)
		{
			return new CustomRegistrar(
				() =>
				{
					var pmlComponent = edgeDecoratorPool[handler]!;
					if (pmlComponent is null) return;
					// 找出属于此PML的pending结点并创建
					foreach (var node in pendingEdgeNodes.Where(n => edgeDataset[n] == pmlComponent).ToList())
					{
						pendingEdgeNodes.Remove(node);
						CreateEdgeNodeView(node, handler);
					}
				},
				() =>
				{
					// PML视图释放时清理该PML下的所有Node视图
					var pmlComponent = edgeDecoratorPool[handler]!;
					foreach (var node in edgeDataset[pmlComponent])
					{
						var nodeHandler = edgeViewPool[node];
						if (edgeViewPool.Remove(node))
							handler.RemovePlugin(nodeHandler!, edgeDecoratorPool.DefaultTransform);
					}
				});
		}

		// ========== Direct侧刷新 ==========

		private void RefreshDirect(T3Time time)
		{
			directGenerator.RefreshTime(time, out var toInstantiate, out var toDestroy);

			foreach (var node in toInstantiate)
			{
				if (directDataset[node] is not { } moveList ||
				    directDecoratorPool[moveList] is not { } decorator)
				{
					pendingDirectNodes.Add(node);
					continue;
				}

				CreateDirectNodeView(node, decorator);
			}

			foreach (var node in toDestroy)
			{
				if (directDataset[node] is not { } moveList ||
				    directDecoratorPool[moveList] is not { } decorator) continue;
				DestroyDirectNodeView(node, decorator);
			}
		}

		private void CreateDirectNodeView(DirectNodeComponent node, PrefabHandler decorator)
		{
			var nodeName = $"{directNodeNamePrefix}{node.Locator.Time.Milli}";
			if (directViewPool.Add(node))
				decorator.AddPlugin(nodeName, directViewPool[node]!);
		}

		private void DestroyDirectNodeView(DirectNodeComponent node, PrefabHandler decorator)
		{
			var nodeHandler = directViewPool[node];
			if (directViewPool.Remove(node))
				decorator.RemovePlugin(nodeHandler!, directDecoratorPool.DefaultTransform);
		}

		// PML视图创建时，处理该PML下的pending结点
		private IEventRegistrar OnDirectPMLViewGet(PrefabHandler handler)
		{
			DirectPMLComponent? pmlComponent = null;
			return new CustomRegistrar(
				() =>
				{
					pmlComponent = directDecoratorPool[handler];
					if (pmlComponent is null) return;
					foreach (var node in pendingDirectNodes.Where(n => directDataset[n] == pmlComponent).ToList())
					{
						pendingDirectNodes.Remove(node);
						CreateDirectNodeView(node, handler);
					}
				},
				() =>
				{
					if (pmlComponent is null) return;
					foreach (var node in directDataset[pmlComponent])
					{
						var nodeHandler = directViewPool[node];
						if (directViewPool.Remove(node))
							handler.RemovePlugin(nodeHandler!, directDecoratorPool.DefaultTransform);
					}
				});
		}

		// ========== ITimeCalculator实现 ==========

		private class EdgeNodeTimeCalculator : ITimeCalculator<EdgeNodeComponent>
		{
			private readonly T3Time windowTime;

			public EdgeNodeTimeCalculator(T3Time windowTime) => this.windowTime = windowTime;

			public T3Time GetTimeInstantiate(EdgeNodeComponent? item)
			{
				if (item is null) return T3Time.MinValue;
				return item.Locator.Time - windowTime;
			}

			public T3Time InstantiateTimeRestriction(T3Time selfTime, T3Time parentTime) => selfTime;

			public T3Time GetTimeDestroy(EdgeNodeComponent? item)
			{
				if (item is null) return T3Time.MaxValue;
				return item.Locator.Time + windowTime;
			}

			public T3Time DestroyTimeRestriction(T3Time selfTime, T3Time parentTime) => selfTime;

			public EdgeNodeComponent? GetParent(EdgeNodeComponent item) => null;

			public IEnumerable<EdgeNodeComponent> GetChildren(EdgeNodeComponent item) =>
				Enumerable.Empty<EdgeNodeComponent>();
		}

		private class DirectNodeTimeCalculator : ITimeCalculator<DirectNodeComponent>
		{
			private readonly T3Time windowTime;

			public DirectNodeTimeCalculator(T3Time windowTime) => this.windowTime = windowTime;

			public T3Time GetTimeInstantiate(DirectNodeComponent? item)
			{
				if (item is null) return T3Time.MinValue;
				return item.Locator.Time - windowTime;
			}

			public T3Time InstantiateTimeRestriction(T3Time selfTime, T3Time parentTime) => selfTime;

			public T3Time GetTimeDestroy(DirectNodeComponent? item)
			{
				if (item is null) return T3Time.MaxValue;
				return item.Locator.Time + windowTime;
			}

			public T3Time DestroyTimeRestriction(T3Time selfTime, T3Time parentTime) => selfTime;

			public DirectNodeComponent? GetParent(DirectNodeComponent item) => null;

			public IEnumerable<DirectNodeComponent> GetChildren(DirectNodeComponent item) =>
				Enumerable.Empty<DirectNodeComponent>();
		}
	}
}