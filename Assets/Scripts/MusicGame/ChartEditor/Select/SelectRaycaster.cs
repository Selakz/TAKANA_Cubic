using System.Linq;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MusicGame.ChartEditor.Select
{
	/// <summary> Dependent on <see cref="SelectManager"/> </summary>
	public class SelectRaycaster : MonoBehaviour
	{
		// Serializable and Public
		public static SelectRaycaster Instance { get; private set; }

		public RaycastMode RaycastMode { get; set; }

		public bool UnselectAllBeforeRaycast { get; set; }

		public int RaycastLayerMask { get; set; }

		// Private

		// Static

		// Defined Functions
		public void DoRaycast(Vector3 screenPoint)
		{
			Ray ray = LevelManager.Instance.LevelCamera.ScreenPointToRay(screenPoint);
			// In this case, you should avoid IEnumerable's lazy loading!!!
			var raycastIds = RaycastMode.DoRaycast(ray, RaycastLayerMask).Select(x => x.Id).ToList();
			if (UnselectAllBeforeRaycast) ISelectManager.Instance.UnselectAll();
			ISelectManager.Instance.ToggleBundle(raycastIds);
		}

		// Event Handlers
		private void OnRaycastNote(InputAction.CallbackContext context)
		{
			var screenPoint = Input.mousePosition;
			if (!LevelManager.Instance.LevelCamera.ContainsScreenPoint(screenPoint)) return;
			UnselectAllBeforeRaycast = true;
			RaycastLayerMask = LayerMask.GetMask("Notes");
			var notNotes = ISelectManager.Instance.SelectedTargets.Keys
				.Where(id => ISelectManager.Instance.SelectedTargets[id] is not EditingNote).ToArray();
			ISelectManager.Instance.UnselectBundle(notNotes);
			DoRaycast(screenPoint);
		}

		private void OnRaycastTrack(InputAction.CallbackContext context)
		{
			var screenPoint = Input.mousePosition;
			if (!LevelManager.Instance.LevelCamera.ContainsScreenPoint(screenPoint)) return;
			UnselectAllBeforeRaycast = true;
			RaycastLayerMask = LayerMask.GetMask("Tracks");
			var notTracks = ISelectManager.Instance.SelectedTargets.Keys
				.Where(id => ISelectManager.Instance.SelectedTargets[id] is not EditingTrack).ToArray();
			ISelectManager.Instance.UnselectBundle(notTracks);
			DoRaycast(screenPoint);
		}

		/// <summary> When pressing ctrl </summary>
		private void OnRaycastNoteMulti(InputAction.CallbackContext context)
		{
			var screenPoint = Input.mousePosition;
			if (!LevelManager.Instance.LevelCamera.ContainsScreenPoint(screenPoint)) return;
			UnselectAllBeforeRaycast = false;
			RaycastLayerMask = LayerMask.GetMask("Notes");
			var notNotes = ISelectManager.Instance.SelectedTargets.Keys
				.Where(id => ISelectManager.Instance.SelectedTargets[id] is not EditingNote).ToArray();
			ISelectManager.Instance.UnselectBundle(notNotes);
			DoRaycast(screenPoint);
		}

		/// <summary> When pressing ctrl </summary>
		private void OnRaycastTrackMulti(InputAction.CallbackContext context)
		{
			var screenPoint = Input.mousePosition;
			if (!LevelManager.Instance.LevelCamera.ContainsScreenPoint(screenPoint)) return;
			UnselectAllBeforeRaycast = false;
			RaycastLayerMask = LayerMask.GetMask("Tracks");
			var notTracks = ISelectManager.Instance.SelectedTargets.Keys
				.Where(id => ISelectManager.Instance.SelectedTargets[id] is not EditingTrack).ToArray();
			ISelectManager.Instance.UnselectBundle(notTracks);
			DoRaycast(screenPoint);
		}

		// System Functions
		void OnEnable()
		{
			Instance = this;
			InputManager.Instance.Register("InScreenEdit", "RaycastNote", OnRaycastNote);
			InputManager.Instance.Register("InScreenEdit", "RaycastTrack", OnRaycastTrack);
			InputManager.Instance.Register("InScreenEdit", "RaycastNoteMulti", OnRaycastNoteMulti);
			InputManager.Instance.Register("InScreenEdit", "RaycastTrackMulti", OnRaycastTrackMulti);
		}
	}
}