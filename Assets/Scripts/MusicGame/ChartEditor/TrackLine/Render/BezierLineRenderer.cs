#nullable enable

using MusicGame.Models.Track.Movement;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine.Render
{
	public class BezierLineRenderer : MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public Material BezierMaterial { get; set; } = default!;

		[field: SerializeField]
		public MeshRenderer LineRenderer { get; set; } = default!;

		[field: SerializeField]
		public MeshCollider? LineCollider { get; set; }

		// Private
		public Vector2 CurrentBuffer { get; private set; }
		public Vector2 NextBuffer { get; private set; }
		public Vector2 StartFactorBuffer { get; private set; }
		public Vector2 EndFactorBuffer { get; private set; }

		private Mesh? logicMesh;
		private readonly V1BMoveItem calculator = new(0, Vector2.zero, Vector2.zero);

		// Static
		private static readonly int bezierFactor = Shader.PropertyToID("_BezierFactor");
		private static readonly int amplitude = Shader.PropertyToID("_Amplitude");
		private static readonly int period = Shader.PropertyToID("_Period");

		// Defined Functions
		public void Init(Vector2 startFactor, Vector2 endFactor, Vector2 current, Vector2 next)
		{
			if (current == CurrentBuffer && next == NextBuffer &&
			    StartFactorBuffer == startFactor && EndFactorBuffer == endFactor &&
			    LineRenderer.material.shader == BezierMaterial.shader) return;

			CurrentBuffer = current;
			NextBuffer = next;
			StartFactorBuffer = startFactor;
			EndFactorBuffer = endFactor;

			if (LineRenderer.material.shader != BezierMaterial.shader)
			{
				var color = LineRenderer.material.color;
				LineRenderer.material = BezierMaterial;
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

			DrawViewMesh(startFactor, endFactor, current, next);
			DrawLogicMesh(startFactor, endFactor, current, next);
		}

		private void DrawViewMesh(Vector2 startFactor, Vector2 endFactor, Vector2 current, Vector2 next)
		{
			LineRenderer.material.SetVector(
				bezierFactor, new(startFactor.x, startFactor.y, endFactor.x, endFactor.y));
			LineRenderer.material.SetFloat(amplitude, next.y - current.y);
			LineRenderer.material.SetFloat(period, next.x - current.x);
			LineRenderer.localBounds = new Bounds((next - current) / 2, next - current);
		}

		private void DrawLogicMesh(Vector2 startFactor, Vector2 endFactor, Vector2 current, Vector2 next)
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
			LineDrawer.DrawMesh(
				logicMesh,
				x => PointCalculator(x, startFactor, endFactor, current, next),
				logicLineWidth,
				next.x - current.x, next.y - current.y,
				logicLinePrecision, maxSegment);
			LineCollider.sharedMesh = logicMesh;
		}

		private float PointCalculator(float x, Vector2 startFactor, Vector2 endFactor, Vector2 current, Vector2 next)
		{
			calculator.StartControlFactor = startFactor;
			calculator.EndControlFactor = endFactor;
			return calculator.GetPosition(0, x, next.y - current.y, next.x - current.x);
		}
	}
}