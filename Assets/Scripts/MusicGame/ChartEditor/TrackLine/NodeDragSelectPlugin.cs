#nullable enable

using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Level;
using T3Framework.Preset.Drag;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Event.UI;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLine
{
	public class NodeSelectionBoxHelper : SelectionBoxHelper
	{
		public override int DragThreshold => ISingleton<EditorSetting>.Instance.MouseDragThreshold;

		public override Vector3 CurrentScreenPoint => Input.mousePosition;

		public NodeSelectionBoxHelper(Camera camera, BoxCollider boxCollider, Plane plane)
			: base(camera, boxCollider, plane)
		{
		}
	}

	public class NodeDragSelectPlugin : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private SequencePriority nodePreviewModuleId = default!;
		[SerializeField] private SequencePriority dragPriority = default!;
		[SerializeField] private SpriteRenderer boxView = default!;
		[SerializeField] private ColliderEnterExitListener listener = default!;
		[SerializeField] private int selectColliderLayer;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(dragHelper.IsDragging, () =>
			{
				if (dragHelper.IsDragging && !isCtrlPressing) dataset.Clear();
			}),
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", dragPriority.Value, OnBeginDragInput,
				InputActionPhase.Started),
			new InputRegistrar("InScreenEdit", "Raycast", "raycast", dragPriority.Value, OnEndDragInput,
				InputActionPhase.Performed),
			new InputRegistrar("General", "Escape", OnCancelDragInput),
			new InputPressingRegistrar("General", "Ctrl", value => isCtrlPressing = value)
		};

		// Private
		private readonly Plane gamePlane = new(Vector3.forward, Vector3.zero);
		private ModuleInfo moduleInfo = default!;
		private Camera levelCamera = default!;
		private EdgeNodeSelectDataset dataset = default!;
		private EdgeNodeSelectInputSystem inputSystem = default!;
		private NodeSelectionBoxHelper dragHelper = default!;

		private bool isCtrlPressing = false;

		// Defined Functions
		[Inject]
		private void Construct(
			ModuleInfo moduleInfo,
			[Key("stage")] Camera levelCamera,
			EdgeNodeSelectDataset dataset,
			EdgeNodeSelectInputSystem inputSystem)
		{
			this.moduleInfo = moduleInfo;
			this.levelCamera = levelCamera;
			this.dataset = dataset;
			this.inputSystem = inputSystem;

			if (listener.Collider is not BoxCollider boxCollider)
			{
				Debug.LogError("NodeDragSelectPlugin needs a BoxCollider.");
				return;
			}

			listener.Layer = selectColliderLayer;
			dragHelper = new(levelCamera, boxCollider, gamePlane);
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Handlers
		private bool OnBeginDragInput()
		{
			if (moduleInfo.CurrentModule != nodePreviewModuleId) return true;
			if (dragHelper.IsDragging.Value) return true;
			if (!levelCamera.ContainsScreenPoint(Input.mousePosition)) return true;

			listener.CollisionEnter += OnCollisionChange;
			listener.CollisionExit += OnCollisionChange;
			return !dragHelper.BeginDrag();
		}

		private bool OnEndDragInput()
		{
			listener.CollisionEnter -= OnCollisionChange;
			listener.CollisionExit -= OnCollisionChange;
			boxView.enabled = false;
			return !dragHelper.EndDrag();
		}

		private void OnCancelDragInput()
		{
			listener.CollisionEnter -= OnCollisionChange;
			listener.CollisionExit -= OnCollisionChange;
			boxView.enabled = false;
			dragHelper.CancelDrag();
		}

		private void OnCollisionChange(Collider collider)
		{
			if (collider is MeshCollider) return; // Exclude line mesh
			var data = inputSystem.System.GetData(collider);
			if (data is not null) dataset.ToggleSelecting(data);
		}

		// System Functions
		void Update()
		{
			if (dragHelper.IsDragging.Value)
			{
				boxView.size = dragHelper.BoxCollider.size;
				boxView.enabled = true;
			}
		}
	}
}