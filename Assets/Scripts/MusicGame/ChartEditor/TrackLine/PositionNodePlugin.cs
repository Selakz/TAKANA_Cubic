#nullable enable

using MusicGame.ChartEditor.TrackLine.Render;
using T3Framework.Runtime.Modifier;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine
{
	public class PositionNodePlugin : MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public AnyLineRenderer LeftRenderer { get; set; } = default!;

		[field: SerializeField]
		public AnyLineRenderer RightRenderer { get; set; } = default!;

		public int SegmentCount => 100;

		public Vector4[] LeftSegments => leftSegments ??= new Vector4[SegmentCount / 2];
		public Vector4[] RightSegments => rightSegments ??= new Vector4[SegmentCount / 2];

		// Private
		private Modifier<Color>? colorModifier;
		private Vector4[]? leftSegments;
		private Vector4[]? rightSegments;

		// Static
		private static readonly int lineWidth = Shader.PropertyToID("_Width");

		// System Functions
		void Awake()
		{
			var setting = ISingletonSetting<TrackLineSetting>.Instance;
			var viewLineWidth = setting.UneditableLineWidth.Value / 2;
			LeftRenderer.LineRenderer.material.SetFloat(lineWidth, viewLineWidth);
			RightRenderer.LineRenderer.material.SetFloat(lineWidth, viewLineWidth);
			LeftRenderer.LineRenderer.material.color = setting.LeftSideNodeColor;
			RightRenderer.LineRenderer.material.color = setting.RightSideNodeColor;
		}
	}
}