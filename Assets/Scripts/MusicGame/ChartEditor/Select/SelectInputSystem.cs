#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Preset.Select;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.Select
{
	[Serializable]
	public struct ChartRaycastConfig
	{
		[field: SerializeField]
		public LayerMask Layer { get; set; }

		[field: SerializeField]
		public SequencePriority InputPriority { get; set; }
	}

	public class SelectInputSystem : HierarchySystem<SelectInputSystem>
	{
		// Serializable and Public
		[SerializeField] private InspectorDictionary<T3Flag, ChartRaycastConfig> raycastConfig = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new UnionRegistrar(RaycastSystemFactory)
		};

		public IReadOnlyDictionary<T3Flag, SelectRaycastSystem<ChartComponent>> RaycastSystems => raycastSystems;

		// Private
		private Camera mainCamera = default!;
		private NotifiableProperty<ISelectRaycastMode<ChartComponent>> raycastMode = default!;
		private ChartSelectDataset dataset = default!;
		private IViewPool<ChartComponent> viewPool = default!;

		private readonly Dictionary<T3Flag, SelectRaycastSystem<ChartComponent>> raycastSystems = new();

		// Constructor
		[Inject]
		private void Construct(
			[Key("stage")] Camera mainCamera,
			NotifiableProperty<ISelectRaycastMode<ChartComponent>> raycastMode,
			ChartSelectDataset dataset,
			[Key("stage")] IViewPool<ChartComponent> viewPool)
		{
			this.mainCamera = mainCamera;
			this.raycastMode = raycastMode;
			this.dataset = dataset;
			this.viewPool = viewPool;
		}

		// Defined Functions
		private IEnumerable<IEventRegistrar> RaycastSystemFactory()
		{
			foreach (var pair in raycastConfig.Value)
			{
				var subDataset = new SubSelectDataset<ChartComponent, T3Flag>(
					dataset, T3ChartClassifier.Instance, pair.Key);
				var system = new ChartRaycastSystem(subDataset, new MouseRayMaker(mainCamera), 100)
				{
					ChartViewPool = viewPool,
					RaycastLayerMask = pair.Value.Layer
				};
				raycastSystems[pair.Key] = system;
				yield return new InputRegistrar("InScreenEdit", "Raycast", "raycast", pair.Value.InputPriority.Value,
					() =>
					{
						if (!mainCamera.ContainsScreenPoint(Input.mousePosition)) return true;
						var span = raycastSystems[pair.Key].DoRaycast(raycastMode.Value);
						return span.Length == 0;
					});
			}
		}
	}
}