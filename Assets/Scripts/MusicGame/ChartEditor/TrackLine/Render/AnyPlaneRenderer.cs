#nullable enable

using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine.Render
{
	public class AnyPlaneRenderer : MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public Material AnyPlaneMaterial { get; set; } = default!;

		[field: SerializeField]
		public MeshRenderer PlaneRenderer { get; set; } = default!;

		[field: SerializeField]
		public MeshCollider? PlaneCollider { get; set; }

		// Private
		private Mesh? logicMesh;

		// Static
		private static readonly int leftSamplePointArray = Shader.PropertyToID("_LeftSamplePointArray");
		private static readonly int rightSamplePointArray = Shader.PropertyToID("_RightSamplePointArray");
		private static readonly int samplePointCountId = Shader.PropertyToID("_SamplePointCount");
		private static readonly int leftAmplitude = Shader.PropertyToID("_LeftAmplitude");
		private static readonly int leftPeriod = Shader.PropertyToID("_LeftPeriod");
		private static readonly int leftOffset = Shader.PropertyToID("_LeftOffset");
		private static readonly int rightAmplitude = Shader.PropertyToID("_RightAmplitude");
		private static readonly int rightPeriod = Shader.PropertyToID("_RightPeriod");
		private static readonly int rightOffset = Shader.PropertyToID("_RightOffset");

		// Defined Functions
		public void Init(
			Vector4[] leftSamplePoints, Vector4[] rightSamplePoints,
			float leftAmplitudeVal, float leftPeriodVal, float leftOffsetVal,
			float rightAmplitudeVal, float rightPeriodVal, float rightOffsetVal)
		{
			if (PlaneRenderer.material.shader != AnyPlaneMaterial.shader)
			{
				var color = PlaneRenderer.material.color;
				PlaneRenderer.material = AnyPlaneMaterial;
				PlaneRenderer.material.color = color;
			}

			transform.localPosition = new(0, 0, -0.01f);

			DrawViewMesh(leftSamplePoints, rightSamplePoints,
				leftAmplitudeVal, leftPeriodVal, leftOffsetVal,
				rightAmplitudeVal, rightPeriodVal, rightOffsetVal);
			DrawLogicMesh(leftSamplePoints, rightSamplePoints,
				leftAmplitudeVal, leftPeriodVal, leftOffsetVal,
				rightAmplitudeVal, rightPeriodVal, rightOffsetVal);
		}

		private void DrawViewMesh(
			Vector4[] leftSamplePoints, Vector4[] rightSamplePoints,
			float leftAmplitudeVal, float leftPeriodVal, float leftOffsetVal,
			float rightAmplitudeVal, float rightPeriodVal, float rightOffsetVal)
		{
			int count = leftSamplePoints.Length * 2;
			PlaneRenderer.material.SetInteger(samplePointCountId, count);
			PlaneRenderer.material.SetVectorArray(leftSamplePointArray, leftSamplePoints);
			PlaneRenderer.material.SetVectorArray(rightSamplePointArray, rightSamplePoints);
			PlaneRenderer.material.SetFloat(leftAmplitude, leftAmplitudeVal);
			PlaneRenderer.material.SetFloat(leftPeriod, leftPeriodVal);
			PlaneRenderer.material.SetFloat(leftOffset, leftOffsetVal);
			PlaneRenderer.material.SetFloat(rightAmplitude, rightAmplitudeVal);
			PlaneRenderer.material.SetFloat(rightPeriod, rightPeriodVal);
			PlaneRenderer.material.SetFloat(rightOffset, rightOffsetVal);

			float minX = Mathf.Min(leftOffsetVal, rightOffsetVal);
			float maxX = Mathf.Max(leftPeriodVal + leftOffsetVal, rightPeriodVal + rightOffsetVal);
			float maxY = Mathf.Max(leftAmplitudeVal, rightAmplitudeVal);
			PlaneRenderer.localBounds = new Bounds(
				new Vector3((minX + maxX) / 2, maxY / 2),
				new Vector3(maxX - minX, maxY));
		}

		private void DrawLogicMesh(
			Vector4[] leftSamplePoints, Vector4[] rightSamplePoints,
			float leftAmplitudeVal, float leftPeriodVal, float leftOffsetVal,
			float rightAmplitudeVal, float rightPeriodVal, float rightOffsetVal)
		{
			if (PlaneCollider == null) return;

			PlaneCollider.enabled = true;
			logicMesh ??= new();

			var leftConverter = new VectorArrayConverter(leftSamplePoints);
			var rightConverter = new VectorArrayConverter(rightSamplePoints);
			var setting = ISingletonSetting<TrackLineSetting>.Instance;
			float precision = setting.LogicLinePrecision.Value;
			int maxSegment = setting.MaxSegment.Value;

			float step = Mathf.Max(precision, 1f / maxSegment);
			int steps = Mathf.FloorToInt(1f / step);

			var vertices = new Vector3[(steps + 1) * 2];
			var tris = new int[steps * 6];

			for (int i = 0; i <= steps; i++)
			{
				float t = Mathf.Min(i * step, 1f);
				float leftX = leftConverter.Interpolate(t) * leftPeriodVal + leftOffsetVal;
				float leftY = t * leftAmplitudeVal;
				float rightX = rightConverter.Interpolate(t) * rightPeriodVal + rightOffsetVal;
				float rightY = t * rightAmplitudeVal;

				float xA = leftX, yA = leftY, xB = rightX, yB = rightY;
				if (xA > xB)
				{
					(xA, xB) = (xB, xA);
					(yA, yB) = (yB, yA);
				}

				xB = Mathf.Max(xB, xA + 0.05f);
				print($"{xA}, {yA}, {xB}, {yB}");

				vertices[i * 2] = new Vector3(xA, yA, 0);
				vertices[i * 2 + 1] = new Vector3(xB, yB, 0);
			}

			for (int i = 0; i < steps; i++)
			{
				int a = i * 2;
				int b = a + 1;
				int c = (i + 1) * 2;
				int d = c + 1;

				tris[i * 6] = a;
				tris[i * 6 + 1] = c;
				tris[i * 6 + 2] = b;
				tris[i * 6 + 3] = b;
				tris[i * 6 + 4] = c;
				tris[i * 6 + 5] = d;
			}

			logicMesh.Clear();
			logicMesh.vertices = vertices;
			logicMesh.triangles = tris;
			logicMesh.RecalculateNormals();

			PlaneCollider.sharedMesh = logicMesh;
		}
	}
}