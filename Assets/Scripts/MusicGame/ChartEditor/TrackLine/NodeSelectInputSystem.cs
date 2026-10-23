#nullable enable

using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.Select;
using T3Framework.Preset.Event;
using T3Framework.Preset.Select;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine
{
	public class NodeSelectInputSystem : HierarchySystem<NodeSelectInputSystem>
	{
		// Serializable and Public
		[SerializeField] private LayerMask raycastLayerMask;
		[SerializeField] private SequencePriority raycastPriority = default!;

		public ViewSelectRaycastSystem<EdgeNodeComponent> EdgeSystem { get; private set; } = default!;
		public ViewSelectRaycastSystem<DirectNodeComponent> DirectSystem { get; private set; } = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", raycastPriority.Value,
				() =>
				{
					if (!levelCamera.ContainsScreenPoint(Mouse.current.position.ReadValue())) return true;
					var edgeSpan = EdgeSystem.DoRaycast(edgeRaycastMode.Value);
					var directSpan = DirectSystem.DoRaycast(directRaycastMode.Value);
					return edgeSpan.Length == 0 && directSpan.Length == 0;
				}),
			new PropertyRegistrar<EdgeNodeComponent?>(edgeDataset.CurrentSelecting, component =>
			{
				if (component is not null)
					selectDataset.CurrentSelecting.Value = component.Locator.Track;
			}),
			new PropertyRegistrar<DirectNodeComponent?>(directDataset.CurrentSelecting, component =>
			{
				if (component is not null)
					selectDataset.CurrentSelecting.Value = component.Locator.Track;
			})
		};

		// Private
		[Inject, Key("stage")] private Camera levelCamera = default!;
		[Inject] private NotifiableProperty<ISelectRaycastMode<EdgeNodeComponent>> edgeRaycastMode = default!;
		[Inject] private NotifiableProperty<ISelectRaycastMode<DirectNodeComponent>> directRaycastMode = default!;
		[Inject] private ChartSelectDataset selectDataset = default!;
		[Inject] private EdgeNodeSelectDataset edgeDataset = default!;
		[Inject] private DirectNodeSelectDataset directDataset = default!;
		[Inject] private IViewPool<EdgeNodeComponent> edgeViewPool = default!;
		[Inject] private IViewPool<DirectNodeComponent> directViewPool = default!;

		// Defined Functions
		[Inject]
		private void Construct()
		{
			EdgeSystem = new(edgeDataset, new MouseRayMaker(levelCamera), 100,
				collider => collider.transform.parent.GetComponent<PrefabHandler>())
			{
				ViewPool = edgeViewPool,
				RaycastLayerMask = raycastLayerMask
			};
			DirectSystem = new(directDataset, new MouseRayMaker(levelCamera), 100,
				collider => collider.transform.parent.GetComponent<PrefabHandler>())
			{
				ViewPool = directViewPool,
				RaycastLayerMask = raycastLayerMask
			};
		}
	}
}