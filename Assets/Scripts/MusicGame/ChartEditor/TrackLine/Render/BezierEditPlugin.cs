#nullable enable

using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine.Render
{
	public class BezierEditPlugin : MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public BoxCollider ControlPoint1 { get; set; } = default!;

		[field: SerializeField]
		public BoxCollider ControlPoint2 { get; set; } = default!;

		// Private
		private BezierLineRenderer TargetRenderer => transform.parent.GetComponent<BezierLineRenderer>();

		private Vector2 currentBuffer;
		private Vector2 nextBuffer;
		private Vector2 startFactorBuffer;
		private Vector2 endFactorBuffer;

		// Defined Functions
		public void Init(Vector2 startFactor, Vector2 endFactor, Vector2 current, Vector2 next)
		{
			currentBuffer = current;
			nextBuffer = next;
			startFactorBuffer = startFactor;
			endFactorBuffer = endFactor;
			TargetRenderer.Init(startFactor, endFactor, current, next);

			ControlPoint1.transform.localPosition = (next - current) * Reverse(startFactor);
			ControlPoint2.transform.localPosition = (next - current) * Reverse(endFactor);
		}

		public Vector2 UpdateCurve(int index, Vector2 position, bool setBuffer = false)
		{
			var point = index switch
			{
				1 => ControlPoint1,
				2 => ControlPoint2,
				_ => null
			};

			if (point is null) return Vector2.zero;
			var newFactor = Reverse((position - currentBuffer) / (nextBuffer - currentBuffer));
			newFactor = newFactor with
			{
				x = Mathf.Clamp01(newFactor.x),
				y = Mathf.Clamp01(newFactor.y)
			};
			point.transform.localPosition = (nextBuffer - currentBuffer) * Reverse(newFactor);

			if (index == 1) TargetRenderer.Init(newFactor, endFactorBuffer, currentBuffer, nextBuffer);
			else TargetRenderer.Init(startFactorBuffer, newFactor, currentBuffer, nextBuffer);

			if (setBuffer)
			{
				if (index == 1) startFactorBuffer = newFactor;
				else endFactorBuffer = newFactor;
			}

			return newFactor;
		}

		public void CancelCurve()
		{
			Init(startFactorBuffer, endFactorBuffer, currentBuffer, nextBuffer);
			TargetRenderer.Init(startFactorBuffer, endFactorBuffer, currentBuffer, nextBuffer);
		}

		private static Vector2 Reverse(Vector2 v) => new(v.y, v.x);
	}
}