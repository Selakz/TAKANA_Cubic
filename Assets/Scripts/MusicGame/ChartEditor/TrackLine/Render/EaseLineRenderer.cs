#nullable enable

using T3Framework.Runtime.Setting;
using T3Framework.Static.Easing;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine.Render
{
	public class EaseLineRenderer : MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public Material EaseMaterial { get; set; } = default!;

		[field: SerializeField]
		public MeshRenderer LineRenderer { get; set; } = default!;

		[field: SerializeField]
		public MeshCollider? LineCollider { get; set; }

		// Private
		private Vector2 currentBuffer;
		private Vector2 nextBuffer;
		private Eases easeBuffer;

		private Mesh? logicMesh;

		// Static
		private static readonly int easeId = Shader.PropertyToID("_EaseId");
		private static readonly int amplitude = Shader.PropertyToID("_Amplitude");
		private static readonly int period = Shader.PropertyToID("_Period");

		// Defined Functions
		public void Init(Eases ease, Vector2 current, Vector2 next)
		{
			if (current == currentBuffer && next == nextBuffer && easeBuffer == ease &&
			    LineRenderer.material.shader == EaseMaterial.shader) return;

			currentBuffer = current;
			nextBuffer = next;
			easeBuffer = ease;

			if (LineRenderer.material.shader != EaseMaterial.shader)
			{
				var color = LineRenderer.material.color;
				LineRenderer.material = EaseMaterial;
				LineRenderer.material.color = color;
			}

			transform.localPosition = new(current.x, current.y, -0.01f);
			if (next.x < current.x)
			{
				next.x = 2 * current.x - next.x;
				var scale = LineRenderer.transform.localScale;
				LineRenderer.transform.localScale = scale with { x = -Mathf.Abs(scale.x) };
			}
			else
			{
				var scale = LineRenderer.transform.localScale;
				LineRenderer.transform.localScale = scale with { x = Mathf.Abs(scale.x) };
			}

			DrawViewMesh(ease, current, next);
			DrawLogicMesh(ease, current, next);
		}

		private void DrawViewMesh(Eases ease, Vector2 current, Vector2 next)
		{
			LineRenderer.material.SetInt(easeId, ease.Opposite().GetId() % 100);
			LineRenderer.material.SetFloat(amplitude, next.y - current.y);
			LineRenderer.material.SetFloat(period, next.x - current.x);
			LineRenderer.localBounds = new Bounds((next - current) / 2, next - current);
		}

		private void DrawLogicMesh(Eases ease, Vector2 current, Vector2 next)
		{
			if (LineCollider is null) return;
			if (current == next)
			{
				LineCollider.enabled = false;
				return;
			}

			LineCollider.enabled = true;
			logicMesh ??= new();
			var setting = ISingletonSetting<TrackLineSetting>.Instance;
			var logicLineWidth = setting.LogicLineWidth.Value;
			var logicLinePrecision = setting.LogicLinePrecision.Value;
			var maxSegment = setting.MaxSegment.Value;
			LineDrawer.DrawMesh(logicMesh, ease.Opposite(), logicLineWidth,
				next.x - current.x, next.y - current.y,
				logicLinePrecision, maxSegment);
			LineCollider.sharedMesh = logicMesh;
		}
	}
}