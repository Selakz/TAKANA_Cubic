#nullable enable

using System;
using T3Framework.Runtime.Modifier;
using UnityEngine;

namespace T3Framework.Preset.Wrapper
{
	[Serializable]
	public class SpriteRendererModifier : RendererModifier
	{
		[SerializeField] private SpriteRenderer spriteRenderer;

		public sealed override Renderer Value => spriteRenderer;

		public Modifier<Sprite> SpriteModifier
		{
			get
			{
				if (spriteModifier is not null) return spriteModifier;
				var defaultSprite = spriteRenderer.sprite;
				spriteModifier = new Modifier<Sprite>(
					() => spriteRenderer.sprite,
					value => spriteRenderer.sprite = value,
					defaultSprite);
				return spriteModifier;
			}
		}

		public override Modifier<int> SortingOrderModifier
		{
			get
			{
				if (sortingOrderModifier is not null) return sortingOrderModifier;
				var defaultSortingOrder = spriteRenderer.sortingOrder;
				sortingOrderModifier = new Modifier<int>(
					() => spriteRenderer.sortingOrder,
					value => spriteRenderer.sortingOrder = value,
					defaultSortingOrder);
				return sortingOrderModifier;
			}
		}

		public Modifier<Vector2> SizeModifier
		{
			get
			{
				if (sizeModifier is not null) return sizeModifier;
				var defaultSize = spriteRenderer.size;
				sizeModifier = new Modifier<Vector2>(
					() => spriteRenderer.size,
					value => spriteRenderer.size = value,
					defaultSize);
				return sizeModifier;
			}
		}

		public override Modifier<Color> ColorModifier
		{
			get
			{
				if (colorModifier is not null) return colorModifier;
				var defaultColor = spriteRenderer.color;
				colorModifier = new Modifier<Color>(
					() => spriteRenderer.color,
					value => spriteRenderer.color = value,
					defaultColor);
				return colorModifier;
			}
		}

		public SpriteRendererModifier(SpriteRenderer spriteRenderer)
		{
			this.spriteRenderer = spriteRenderer;
		}

		private Modifier<Sprite>? spriteModifier;

		private Modifier<int>? sortingOrderModifier;

		private Modifier<Vector2>? sizeModifier;

		private Modifier<Color>? colorModifier;
	}
}