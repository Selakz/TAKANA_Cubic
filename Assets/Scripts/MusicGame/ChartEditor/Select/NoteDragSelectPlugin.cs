#nullable enable

using System.Collections.Generic;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
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

namespace MusicGame.ChartEditor.Select
{
	public class NoteSelectionBoxHelper : SelectionBoxHelper
	{
		public override int DragThreshold => ISingleton<EditorSetting>.Instance.MouseDragThreshold;

		public override Vector3 CurrentScreenPoint => Mouse.current.position.ReadValue();

		public NoteSelectionBoxHelper(Camera camera, BoxCollider boxCollider, Plane plane)
			: base(camera, boxCollider, plane)
		{
		}
	}

	public class NoteDragSelectPlugin : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
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
		private readonly HashSet<ChartComponent> pendingToggleComponents = new();
		private Camera levelCamera = default!;
		private ChartSelectDataset dataset = default!;
		private SelectInputSystem inputSystem = default!;
		private NoteSelectionBoxHelper dragHelper = default!;

		private bool isCtrlPressing = false;

		// Defined Functions
		[Inject]
		private void Construct(
			[Key("stage")] Camera levelCamera,
			ChartSelectDataset dataset,
			SelectInputSystem inputSystem)
		{
			this.levelCamera = levelCamera;
			this.dataset = dataset;
			this.inputSystem = inputSystem;

			if (listener.Collider is not BoxCollider boxCollider)
			{
				Debug.LogError("NoteDragSelectPlugin needs a BoxCollider.");
				return;
			}

			listener.Layer = selectColliderLayer;
			dragHelper = new(levelCamera, boxCollider, gamePlane);
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Handlers
		private bool OnBeginDragInput()
		{
			if (dragHelper.IsDragging.Value) return true;
			if (!levelCamera.ContainsScreenPoint(Mouse.current.position.ReadValue())) return true;
			if (!inputSystem.RaycastSystems.TryGetValue(T3Flag.Note, out _)) return true;

			pendingToggleComponents.Clear();
			listener.CollisionEnter += OnCollisionChange;
			listener.CollisionExit += OnCollisionChange;
			return !dragHelper.BeginDrag();
		}

		private bool OnEndDragInput()
		{
			listener.CollisionEnter -= OnCollisionChange;
			listener.CollisionExit -= OnCollisionChange;
			boxView.enabled = false;
			FlushPendingToggles();
			return !dragHelper.EndDrag();
		}

		private void OnCancelDragInput()
		{
			listener.CollisionEnter -= OnCollisionChange;
			listener.CollisionExit -= OnCollisionChange;
			boxView.enabled = false;
			pendingToggleComponents.Clear();
			dragHelper.CancelDrag();
		}

		private void OnCollisionChange(Collider collider)
		{
			if (!inputSystem.RaycastSystems.TryGetValue(T3Flag.Note, out var raycastSystem)) return;

			var data = raycastSystem.GetData(collider);
			if (data is null) return;
			// Defer dataset mutation out of physics trigger callbacks to avoid SetActive(false) destroying TMP meshes immediately.
			if (!pendingToggleComponents.Add(data)) pendingToggleComponents.Remove(data);
		}

		private void FlushPendingToggles()
		{
			if (pendingToggleComponents.Count == 0) return;

			foreach (var component in pendingToggleComponents)
			{
				dataset.ToggleSelecting(component);
			}

			pendingToggleComponents.Clear();
		}

		// System Functions
		void Update()
		{
			FlushPendingToggles();
			if (dragHelper.IsDragging.Value)
			{
				boxView.size = dragHelper.BoxCollider.size;
				boxView.enabled = true;
			}
		}
	}
}