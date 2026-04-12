#nullable enable

using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine.Render
{
	public class AnyLineRenderer : MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public Material AnyMaterial { get; set; } = default!;

		[field: SerializeField]
		public MeshRenderer LineRenderer { get; set; } = default!;

		[field: SerializeField]
		public MeshCollider? LineCollider { get; set; }

		// Static
		private static readonly int samplePointArray = Shader.PropertyToID("_SamplePointArray");
		private static readonly int samplePointCount = Shader.PropertyToID("_SamplePointCount");
		private static readonly int amplitude = Shader.PropertyToID("_Amplitude");
		private static readonly int period = Shader.PropertyToID("_Period");

		// Defined Functions
		public void Init(Vector4[] samplePoints, Vector2 current, Vector2 next)
		{
			if (LineRenderer.material.shader != AnyMaterial.shader)
			{
				var color = LineRenderer.material.color;
				LineRenderer.material = AnyMaterial;
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

			DrawViewMesh(samplePoints, current, next);
			// TODO: DrawLogicMesh
		}

		private void DrawViewMesh(Vector4[] samplePoints, Vector2 current, Vector2 next)
		{
			LineRenderer.material.SetInteger(samplePointCount, samplePoints.Length * 2);
			LineRenderer.material.SetVectorArray(samplePointArray, samplePoints);
			LineRenderer.material.SetFloat(amplitude, next.y - current.y);
			LineRenderer.material.SetFloat(period, next.x - current.x);
			LineRenderer.localBounds = new Bounds((next - current) / 2, next - current);
		}
	}
}