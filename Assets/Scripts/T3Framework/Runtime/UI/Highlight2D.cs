using UnityEngine;

namespace T3Framework.Runtime.UI
{
	// TODO: Delete this
	[RequireComponent(typeof(SpriteRenderer))]
	public class Highlight2D : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Sprite highlightSprite;
		[SerializeField] private int highlightLayer = -1;

		public Sprite HighlightSprite
		{
			get => highlightSprite;
			set => highlightSprite = value;
		}

		public int HighlightLayer
		{
			get => highlightLayer;
			set => highlightLayer = value;
		}

		public bool IsHighlight
		{
			get => isHighlight;
			set
			{
				if (isHighlight == value) return;
				if (spriteRenderer != null)
				{
					if (value)
					{
						spriteRenderer.sprite = HighlightSprite;
						spriteRenderer.sortingOrder = HighlightLayer;
					}
					else
					{
						spriteRenderer.sprite = originalSprite;
						spriteRenderer.sortingOrder = originalLayer;
					}
				}
				else
				{
					Debug.Log("sprite is null");
				}

				isHighlight = value;
			}
		}

		// Private
		private SpriteRenderer spriteRenderer;
		private Sprite originalSprite;
		private int originalLayer;
		private bool isHighlight = false;

		// Defined Functions
		/// <summary> If not calling it, it will be called in <see cref="Awake"/>. </summary>
		public void Init()
		{
			spriteRenderer = GetComponent<SpriteRenderer>();
			originalSprite = spriteRenderer.sprite;
			originalLayer = spriteRenderer.sortingOrder;
			if (HighlightLayer < 0) HighlightLayer = originalLayer;
		}

		// System Functions
		void Awake()
		{
			if (spriteRenderer == null)
			{
				Init();
			}
		}
	}
}