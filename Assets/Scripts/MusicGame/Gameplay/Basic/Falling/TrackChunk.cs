#nullable enable

using MusicGame.ChartEditor.TrackLine.Render;
using UnityEngine;

namespace MusicGame.Gameplay.Basic.Falling
{
	public class TrackChunk : MonoBehaviour
	{
		[SerializeField] private float lineWidth = 0.03f;

		[field: SerializeField]
		public AnyPlaneRenderer Plane { get; private set; } = default!;

		[field: SerializeField]
		public AnyLineRenderer LeftLine { get; private set; } = default!;

		[field: SerializeField]
		public AnyLineRenderer RightLine { get; private set; } = default!;

		public void Clear()
		{
			gameObject.SetActive(false);
			transform.SetParent(null, false);
		}

		void Awake()
		{
			var lineWidthId = Shader.PropertyToID("_Width");
			LeftLine.LineRenderer.material.SetFloat(lineWidthId, lineWidth);
			RightLine.LineRenderer.material.SetFloat(lineWidthId, lineWidth);
		}
	}
}