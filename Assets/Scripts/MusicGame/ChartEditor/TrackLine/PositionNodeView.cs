#nullable enable

using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Easing;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine
{
	// TODO: Make it abstract and support other form of movement
	public class PositionNodeView : MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public SpriteRenderer NodeRenderer { get; set; } = default!;

		[field: SerializeField]
		public MeshRenderer LineRenderer { get; set; } = default!;

		[field: SerializeField]
		public BoxCollider NodeCollider { get; set; } = default!;

		[field: SerializeField]
		public MeshCollider LineCollider { get; set; } = default!;

		public bool IsEditable
		{
			get => isEditable;
			set
			{
				var setting = ISingletonSetting<TrackLineSetting>.Instance;
				var viewLineWidth = value
					? setting.EditableLineWidth.Value
					: setting.UneditableLineWidth.Value;
				LineRenderer.material.SetFloat(lineWidth, viewLineWidth);

				if (isEditable == value) return;
				isEditable = value;
				NodeCollider.gameObject.SetActive(value);
				LineCollider.enabled = value;
			}
		}

		public Modifier<Color> ColorModifier => colorModifier ??= new(
			() => NodeRenderer.color,
			color =>
			{
				NodeRenderer.color = color;
				LineRenderer.material.color = color;
			},
			_ => Color.white);


		// Private
		private Vector2 currentBuffer;
		private Vector2 nextBuffer;
		private Eases easeBuffer;

		private Modifier<Color>? colorModifier;
		private bool isEditable = true;
		private Mesh? logicMesh;

		// Static
		private static readonly int easeId = Shader.PropertyToID("_EaseId");
		private static readonly int amplitude = Shader.PropertyToID("_Amplitude");
		private static readonly int period = Shader.PropertyToID("_Period");
		private static readonly int lineWidth = Shader.PropertyToID("_Width");

		// Defined Functions
		public void Init(Eases ease, Vector2 current, Vector2 next)
		{
			if (current == currentBuffer && next == nextBuffer && easeBuffer == ease) return;

			currentBuffer = current;
			nextBuffer = next;
			easeBuffer = ease;

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