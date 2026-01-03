#nullable enable

using System;
using T3Framework.Runtime;
using UnityEngine;

namespace T3Framework.Preset.Wrapper
{
	[Serializable]
	public class SpriteRendererModifier
	{
		[SerializeField] private SpriteRenderer spriteRenderer;

		public SpriteRenderer Value => spriteRenderer;

		public Modifier<Sprite> SpriteModifier
		{
			get
			{
				var defaultSprite = spriteRenderer.sprite;
				return spriteModifier ??= new Modifier<Sprite>(
					() => spriteRenderer.sprite,
					value => spriteRenderer.sprite = value,
					_ => defaultSprite);
			}
		}

		public Modifier<int> SortingOrderModifier
		{
			get
			{
				var defaultSortingOrder = spriteRenderer.sortingOrder;
				return sortingOrderModifier ??= new Modifier<int>(
					() => spriteRenderer.sortingOrder,
					value => spriteRenderer.sortingOrder = value,
					_ => defaultSortingOrder);
			}
		}

		public Modifier<Vector2> SizeModifier
		{
			get
			{
				var defaultSize = spriteRenderer.size;
				return sizeModifier ??= new Modifier<Vector2>(
					() => spriteRenderer.size,
					value => spriteRenderer.size = value,
					_ => defaultSize);
			}
		}

		public Modifier<Color> ColorModifier
		{
			get
			{
				var defaultColor = spriteRenderer.color;
				return colorModifier ??= new Modifier<Color>(
					() => spriteRenderer.color,
					value => spriteRenderer.color = value,
					_ => defaultColor);
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