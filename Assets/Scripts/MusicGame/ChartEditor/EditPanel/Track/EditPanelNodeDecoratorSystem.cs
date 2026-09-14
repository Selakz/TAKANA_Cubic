#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using T3Framework.Preset.UICollection;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.EditPanel.Track
{
	/// <summary>
	/// 编辑面板UI列表项的延迟实例化。
	/// 根据PML视图对应EdgeComponent上的Collapsable.IsCollapsed，决定是否创建Node列表项视图。
	/// Collapsable折叠时移除Node视图，展开时重新创建。
	/// </summary>
	public class EditPanelNodeDecoratorSystem : HierarchySystem<EditPanelNodeDecoratorSystem>
	{
		// Serializable
		[SerializeField] private string edgeNodeNamePrefix = default!;
		[SerializeField] private string directNodeNamePrefix = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			// === Edge侧 ===
			// PML视图创建/释放时注册/注销Collapsable监听，控制Node视图生成
			new ViewPoolLifetimeRegistrar<EdgePMLComponent>(edgeDecoratorPool, OnEdgePMLViewGet, true),
			// Node数据单独加入时检查Collapsable，未折叠则生成视图
			new DatasetRegistrar<EdgeNodeComponent>(edgeDataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataAdded,
				OnEdgeNodeDataAdded),
			// Node数据移除时清理视图
			new DatasetRegistrar<EdgeNodeComponent>(edgeDataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataRemoved,
				OnEdgeNodeDataRemoved),

			// === Direct侧 ===
			// PML视图创建/释放时注册/注销Collapsable监听，控制Node视图生成
			new ViewPoolLifetimeRegistrar<DirectPMLComponent>(directDecoratorPool, OnDirectPMLViewGet, true),
			// Node数据单独加入时检查Collapsable，未折叠则生成视图
			new DatasetRegistrar<DirectNodeComponent>(directDataset,
				DatasetRegistrar<DirectNodeComponent>.RegisterTarget.DataAdded,
				OnDirectNodeDataAdded),
			// Node数据移除时清理视图
			new DatasetRegistrar<DirectNodeComponent>(directDataset,
				DatasetRegistrar<DirectNodeComponent>.RegisterTarget.DataRemoved,
				OnDirectNodeDataRemoved),
		};

		// Private
		private EdgeNodeDataset edgeDataset = default!;
		private IViewPool<EdgePMLComponent> edgeDecoratorPool = default!;
		private IViewPool<EdgeNodeComponent> edgeViewPool = default!;
		private DirectNodeDataset directDataset = default!;
		private IViewPool<DirectPMLComponent> directDecoratorPool = default!;
		private IViewPool<DirectNodeComponent> directViewPool = default!;

		// 每个PML handler对应一个Collapsable上下文（通过handler.Parent链获取Collapsable）
		private readonly Dictionary<PrefabHandler, EdgePMLContext> edgePMLContexts = new();
		private readonly Dictionary<PrefabHandler, DirectPMLContext> directPMLContexts = new();

		// Constructor
		[Inject]
		private void Construct(
			EdgeNodeDataset edgeDataset,
			IViewPool<EdgePMLComponent> edgeDecoratorPool,
			IViewPool<EdgeNodeComponent> edgeViewPool,
			DirectNodeDataset directDataset,
			IViewPool<DirectPMLComponent> directDecoratorPool,
			IViewPool<DirectNodeComponent> directViewPool)
		{
			this.edgeDataset = edgeDataset;
			this.edgeDecoratorPool = edgeDecoratorPool;
			this.edgeViewPool = edgeViewPool;
			this.directDataset = directDataset;
			this.directDecoratorPool = directDecoratorPool;
			this.directViewPool = directViewPool;
		}

		// ========== Edge侧PML生命周期 ==========

		private IEventRegistrar OnEdgePMLViewGet(PrefabHandler handler)
		{
			// 通过Parent链找到EdgeComponent handler上的Collapsable
			var collapsable = handler.Parent!.Script<Collapsable>();
			EdgePMLComponent? pmlComponent = null;
			var context = new EdgePMLContext { Collapsable = collapsable };
			edgePMLContexts[handler] = context;

			return new CustomRegistrar(
				() =>
				{
					pmlComponent = edgeDecoratorPool[handler];
					if (pmlComponent is null) return;
					// 订阅Collapsable状态变化：折叠时移除Node，展开时重新创建
					PropertyChangedEventHandler onChanged = (_, _) =>
					{
						if (collapsable.IsCollapsed.Value)
							RemoveAllEdgeNodeViews(handler, pmlComponent);
						else
							CreateAllEdgeNodeViews(handler, pmlComponent);
					};
					context.OnChangedHandler = onChanged;
					collapsable.IsCollapsed.PropertyChanged += onChanged;

					// 初始加载：仅在未折叠时创建
					if (!collapsable.IsCollapsed.Value)
						CreateAllEdgeNodeViews(handler, pmlComponent);
				},
				() =>
				{
					// PML视图释放时：取消Collapsable订阅，清理所有Node视图
					if (context.Collapsable is not null && context.OnChangedHandler is not null)
						context.Collapsable.IsCollapsed.PropertyChanged -= context.OnChangedHandler;
					if (pmlComponent is not null)
						RemoveAllEdgeNodeViews(handler, pmlComponent);
					edgePMLContexts.Remove(handler);
				});
		}

		// 为指定EdgePML创建该PML下的全部Node视图
		private void CreateAllEdgeNodeViews(PrefabHandler pmlHandler, EdgePMLComponent pmlComponent)
		{
			foreach (var node in edgeDataset[pmlComponent])
			{
				var nodeName = $"{edgeNodeNamePrefix}{node.Locator.Time.Milli}";
				if (edgeViewPool.Add(node))
					pmlHandler.AddPlugin(nodeName, edgeViewPool[node]!);
			}
		}

		// 移除指定EdgePML下的全部Node视图
		private void RemoveAllEdgeNodeViews(PrefabHandler pmlHandler, EdgePMLComponent pmlComponent)
		{
			foreach (var node in edgeDataset[pmlComponent])
			{
				var nodeHandler = edgeViewPool[node];
				if (edgeViewPool.Remove(node))
					pmlHandler.RemovePlugin(nodeHandler!, edgeDecoratorPool.DefaultTransform);
			}
		}

		// Node数据单独加入Edge数据集
		private void OnEdgeNodeDataAdded(EdgeNodeComponent component)
		{
			if (edgeDataset[component] is not { } moveList ||
			    edgeDecoratorPool[moveList] is not { } decorator) return;

			// 检查Collapsable：折叠时不创建Node视图（展开时PML的ViewPoolLifetimeRegistrar会负责创建全部）
			if (edgePMLContexts.TryGetValue(decorator, out var context) &&
			    context.Collapsable.IsCollapsed.Value) return;

			var nodeName = $"{edgeNodeNamePrefix}{component.Locator.Time.Milli}";
			if (edgeViewPool.Add(component))
				decorator.AddPlugin(nodeName, edgeViewPool[component]!);
		}

		// Node数据从Edge数据集移除
		private void OnEdgeNodeDataRemoved(EdgeNodeComponent component)
		{
			if (edgeDataset[component] is not { } moveList ||
			    edgeDecoratorPool[moveList] is not { } decorator) return;
			var nodeHandler = edgeViewPool[component];
			if (edgeViewPool.Remove(component))
				decorator.RemovePlugin(nodeHandler!, edgeDecoratorPool.DefaultTransform);
		}

		// ========== Direct侧PML生命周期 ==========

		private IEventRegistrar OnDirectPMLViewGet(PrefabHandler handler)
		{
			var collapsable = handler.Parent!.Script<Collapsable>();
			DirectPMLComponent? pmlComponent = null;
			var context = new DirectPMLContext { Collapsable = collapsable };
			directPMLContexts[handler] = context;

			return new CustomRegistrar(
				() =>
				{
					pmlComponent = directDecoratorPool[handler];
					if (pmlComponent is null) return;
					PropertyChangedEventHandler onChanged = (_, _) =>
					{
						if (collapsable.IsCollapsed.Value)
							RemoveAllDirectNodeViews(handler, pmlComponent);
						else
							CreateAllDirectNodeViews(handler, pmlComponent);
					};
					context.OnChangedHandler = onChanged;
					collapsable.IsCollapsed.PropertyChanged += onChanged;

					if (!collapsable.IsCollapsed.Value)
						CreateAllDirectNodeViews(handler, pmlComponent);
				},
				() =>
				{
					if (context.Collapsable is not null && context.OnChangedHandler is not null)
						context.Collapsable.IsCollapsed.PropertyChanged -= context.OnChangedHandler;
					if (pmlComponent is not null)
						RemoveAllDirectNodeViews(handler, pmlComponent);
					directPMLContexts.Remove(handler);
				});
		}

		private void CreateAllDirectNodeViews(PrefabHandler pmlHandler, DirectPMLComponent pmlComponent)
		{
			foreach (var node in directDataset[pmlComponent])
			{
				var nodeName = $"{directNodeNamePrefix}{node.Locator.Time.Milli}";
				if (directViewPool.Add(node))
					pmlHandler.AddPlugin(nodeName, directViewPool[node]!);
			}
		}

		private void RemoveAllDirectNodeViews(PrefabHandler pmlHandler, DirectPMLComponent pmlComponent)
		{
			foreach (var node in directDataset[pmlComponent])
			{
				var nodeHandler = directViewPool[node];
				if (directViewPool.Remove(node))
					pmlHandler.RemovePlugin(nodeHandler!, directDecoratorPool.DefaultTransform);
			}
		}

		private void OnDirectNodeDataAdded(DirectNodeComponent component)
		{
			if (directDataset[component] is not { } moveList ||
			    directDecoratorPool[moveList] is not { } decorator) return;

			if (directPMLContexts.TryGetValue(decorator, out var context) &&
			    context.Collapsable.IsCollapsed.Value) return;

			var nodeName = $"{directNodeNamePrefix}{component.Locator.Time.Milli}";
			if (directViewPool.Add(component))
				decorator.AddPlugin(nodeName, directViewPool[component]!);
		}

		private void OnDirectNodeDataRemoved(DirectNodeComponent component)
		{
			if (directDataset[component] is not { } moveList ||
			    directDecoratorPool[moveList] is not { } decorator) return;
			var nodeHandler = directViewPool[component];
			if (directViewPool.Remove(component))
				decorator.RemovePlugin(nodeHandler!, directDecoratorPool.DefaultTransform);
		}

		// ========== 辅助类型 ==========

		private class EdgePMLContext
		{
			public Collapsable Collapsable = default!;
			public PropertyChangedEventHandler? OnChangedHandler;
		}

		private class DirectPMLContext
		{
			public Collapsable Collapsable = default!;
			public PropertyChangedEventHandler? OnChangedHandler;
		}
	}
}