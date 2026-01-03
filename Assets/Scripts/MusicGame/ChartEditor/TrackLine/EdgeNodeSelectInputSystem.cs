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
	public class EdgeNodeSelectInputSystem : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private LayerMask raycastLayerMask;
		[SerializeField] private SequencePriority raycastPriority = default!;

		public EdgeNodeRaycastSystem System { get; private set; } = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", raycastPriority.Value,
				() =>
				{
					if (!levelCamera.ContainsScreenPoint(Input.mousePosition)) return true;
					var span = System.DoRaycast(raycastMode.Value);
					return span.Length == 0;
				})
		};

		// Private
		private Camera levelCamera = default!;
		private NotifiableProperty<ISelectRaycastMode<EdgeNodeComponent>> raycastMode = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			[Key("stage")] Camera levelCamera,
			NotifiableProperty<ISelectRaycastMode<EdgeNodeComponent>> raycastMode,
			EdgeNodeSelectDataset dataset,
			IViewPool<EdgeNodeComponent> viewPool)
		{
			this.levelCamera = levelCamera;
			this.raycastMode = raycastMode;
			System = new EdgeNodeRaycastSystem(dataset, new MouseRayMaker(levelCamera), 100)
			{
				NodeViewPool = viewPool,
				RaycastLayerMask = raycastLayerMask
			};
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}