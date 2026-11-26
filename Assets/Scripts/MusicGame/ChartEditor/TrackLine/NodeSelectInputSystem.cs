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

		public ViewSelectRaycastSystem<TrackNodeComponent> System { get; private set; } = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", raycastPriority.Value,
				() =>
				{
					if (!levelCamera.ContainsScreenPoint(Mouse.current.position.ReadValue())) return true;
					var span = System.DoRaycast(raycastMode.Value);
					return span.Length == 0;
				}),
			new PropertyRegistrar<TrackNodeComponent?>(nodeSelectDataset.CurrentSelecting, node =>
			{
				if (node is not null)
					selectDataset.CurrentSelecting.Value = node.Track;
			})
		};

		// Private
		[Inject, Key("stage")] private readonly Camera levelCamera = default!;
		[Inject] private readonly NotifiableProperty<ISelectRaycastMode<TrackNodeComponent>> raycastMode = default!;
		[Inject] private readonly ChartSelectDataset selectDataset = default!;
		[Inject] private readonly TrackNodeSelectDataset nodeSelectDataset = default!;
		[Inject] private readonly IViewPool<TrackNodeComponent> nodeViewPool = default!;

		// Constructor
		[Inject]
		private void Construct()
		{
			System = new(nodeSelectDataset, new MouseRayMaker(levelCamera), 100,
				collider => collider.transform.parent.GetComponent<PrefabHandler>())
			{
				ViewPool = nodeViewPool,
				RaycastLayerMask = raycastLayerMask
			};
		}
	}
}
