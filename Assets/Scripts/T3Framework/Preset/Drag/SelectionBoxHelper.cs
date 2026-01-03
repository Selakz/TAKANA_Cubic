#nullable enable

using T3Framework.Runtime.Extensions;
using UnityEngine;

namespace T3Framework.Preset.Drag
{
	public abstract class SelectionBoxHelper : DragHelper
	{
		private readonly Camera camera;
		private readonly Plane plane;

		private Vector3 worldStartPoint;
		private Vector3 worldEndPoint;

		public BoxCollider BoxCollider { get; }

		protected SelectionBoxHelper(Camera camera, BoxCollider boxCollider, Plane plane)
		{
			this.camera = camera;
			this.plane = plane;

			BoxCollider = boxCollider;
			BoxCollider.isTrigger = true;
			BoxCollider.enabled = false;
		}

		protected override void BeginDragLogic() => worldStartPoint = GetMouseWorldPosition();

		protected override void OnDraggingLogic()
		{
			worldEndPoint = GetMouseWorldPosition();
			UpdateBoxCollider(worldStartPoint, worldEndPoint);
		}

		protected override void EndDragLogic() => BoxCollider.enabled = false;

		protected override void CancelDragLogic() => BoxCollider.enabled = false;

		private void UpdateBoxCollider(Vector3 p1, Vector3 p2)
		{
			BoxCollider.enabled = true;

			Vector3 center = (p1 + p2) / 2f;
			Vector3 worldSize = new Vector3(Mathf.Abs(p1.x - p2.x), Mathf.Abs(p1.y - p2.y), 1f);
			Vector3 lossyScale = BoxCollider.transform.lossyScale;

			BoxCollider.transform.position = center;
			BoxCollider.size = new Vector3(
				worldSize.x / lossyScale.x,
				worldSize.y / lossyScale.y,
				worldSize.z / lossyScale.z
			);
		}

		private Vector3 GetMouseWorldPosition()
		{
			var mousePoint = CurrentScreenPoint;
			if (!camera.ContainsScreenPoint(mousePoint) ||
			    !camera.ScreenToWorldPoint(plane, mousePoint, out var worldPoint)) return Vector3.zero;
			return worldPoint;
		}
	}
}