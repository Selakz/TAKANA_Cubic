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

	public class SelectInputSystem : T3System
	{
		// Serializable and Public
		protected override IEventRegistrar[] ActiveRegistrars { get; }

		public IReadOnlyDictionary<T3Flag, SelectRaycastSystem<ChartComponent>> RaycastSystems => raycastSystems;

		// Private
		private readonly Dictionary<T3Flag, SelectRaycastSystem<ChartComponent>> raycastSystems = new();

		// Defined Functions
		public SelectInputSystem(
			[Key("select-input")] Dictionary<T3Flag, ChartRaycastConfig> raycastConfig,
			[Key("stage")] Camera camera,
			NotifiableProperty<ISelectRaycastMode<ChartComponent>> raycastMode,
			ChartSelectDataset dataset,
			[Key("stage")] IViewPool<ChartComponent> viewPool) : base(true)
		{
			// Raycast Systems
			List<IEventRegistrar> registrars = new();
			T3ChartClassifier classifier = new();
			foreach (var pair in raycastConfig)
			{
				var subDataset = new SubSelectDataset<ChartComponent, T3Flag>(dataset, classifier, pair.Key);
				var system = new ChartRaycastSystem(subDataset, new MouseRayMaker(camera), 100)
				{
					ChartViewPool = viewPool,
					RaycastLayerMask = pair.Value.Layer
				};
				raycastSystems[pair.Key] = system;
				registrars.Add(new InputRegistrar("InScreenEdit", "Raycast", "raycast", pair.Value.InputPriority.Value,
					() =>
					{
						if (!camera.ContainsScreenPoint(Input.mousePosition)) return true;
						var span = raycastSystems[pair.Key].DoRaycast(raycastMode.Value);
						return span.Length == 0;
					}));
			}

			ActiveRegistrars = registrars.ToArray();
		}
	}
}