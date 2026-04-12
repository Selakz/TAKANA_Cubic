#nullable enable

using MusicGame.ChartEditor.Decoration.Track;
using T3Framework.Preset.Select;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLine
{
	public class NodeSelectInputSystem : T3MonoBehaviour, ISelfInstaller
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
					if (!levelCamera.ContainsScreenPoint(Input.mousePosition)) return true;
					var edgeSpan = EdgeSystem.DoRaycast(edgeRaycastMode.Value);
					var directSpan = DirectSystem.DoRaycast(directRaycastMode.Value);
					return edgeSpan.Length == 0 && directSpan.Length == 0;
				})
		};

		// Private
		private Camera levelCamera = default!;
		private NotifiableProperty<ISelectRaycastMode<EdgeNodeComponent>> edgeRaycastMode = default!;
		private NotifiableProperty<ISelectRaycastMode<DirectNodeComponent>> directRaycastMode = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			[Key("stage")] Camera levelCamera,
			NotifiableProperty<ISelectRaycastMode<EdgeNodeComponent>> edgeRaycastMode,
			NotifiableProperty<ISelectRaycastMode<DirectNodeComponent>> directRaycastMode,
			EdgeNodeSelectDataset edgeDataset,
			DirectNodeSelectDataset directDataset,
			IViewPool<EdgeNodeComponent> edgeViewPool,
			IViewPool<DirectNodeComponent> directViewPool)
		{
			this.levelCamera = levelCamera;
			this.edgeRaycastMode = edgeRaycastMode;
			this.directRaycastMode = directRaycastMode;
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

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}