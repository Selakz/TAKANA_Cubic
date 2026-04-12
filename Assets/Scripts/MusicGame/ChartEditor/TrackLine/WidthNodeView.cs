#nullable enable

using T3Framework.Runtime.Modifier;
using T3Framework.Runtime.Setting;
using TMPro;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine
{
	public class WidthNodeView : MonoBehaviour, INodeView
	{
		// Serializable and Public
		[field: SerializeField]
		public SpriteRenderer NodeRenderer { get; set; } = default!;

		[field: SerializeField]
		public SpriteRenderer LineRenderer { get; set; } = default!;

		[field: SerializeField]
		public BoxCollider NodeCollider { get; set; } = default!;

		[field: SerializeField]
		public BoxCollider LineCollider { get; set; } = default!;

		[field: SerializeField]
		public TextMeshPro LabelText { get; set; } = default!;

		public bool IsEditable
		{
			get => isEditable;
			set
			{
				var setting = ISingletonSetting<TrackLineSetting>.Instance;
				var viewLineWidth = value
					? setting.EditableLineWidth.Value
					: setting.UneditableLineWidth.Value;
				LineRenderer.size = LineRenderer.size with { y = viewLineWidth * 2 };

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
				LineRenderer.color = color;
				LabelText.color = color;
			},
			_ => Color.white);


		// Private
		private string labelBuffer = string.Empty;
		private Vector2 currentBuffer;
		private float widthBuffer;

		private Modifier<Color>? colorModifier;
		private bool isEditable = true;

		// Defined Functions
		public void Init(string label, Vector2 current, float width)
		{
			if (current == currentBuffer && Mathf.Approximately(width, widthBuffer) && label == labelBuffer) return;

			currentBuffer = current;
			widthBuffer = width;
			labelBuffer = label;

			transform.localPosition = new(current.x, current.y, -0.01f);
			LineRenderer.size = LineRenderer.size with { x = width };
			NodeRenderer.transform.localPosition =
				NodeRenderer.transform.localPosition with { x = Mathf.Max(width / 2, NodeRenderer.size.x / 2) };
			LineCollider.size = LineCollider.size with { x = width };
			LabelText.text = label;
			LabelText.transform.localPosition =
				LabelText.transform.localPosition with { x = -width / 2 + LabelText.preferredWidth / 2 + 0.05f };
		}

		// System Functions
		void Awake()
		{
			LineRenderer.sortingLayerID = NodeRenderer.sortingLayerID;
			LineRenderer.sortingOrder = NodeRenderer.sortingOrder;
		}
	}
}