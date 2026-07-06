#nullable enable

using System;
using T3Framework.Runtime.Modifier;
using UnityEngine;

namespace T3Framework.Preset.Wrapper
{
	[Serializable]
	public class MeshRendererModifier : RendererModifier
	{
		[SerializeField] private MeshRenderer meshRenderer;

		public sealed override Renderer Value => meshRenderer;

		public override Modifier<int> SortingOrderModifier
		{
			get
			{
				var defaultSortingOrder = meshRenderer.sortingOrder;
				return sortingOrderModifier ??= new Modifier<int>(
					() => meshRenderer.sortingOrder,
					value => meshRenderer.sortingOrder = value,
					_ => defaultSortingOrder);
			}
		}

		public override Modifier<Color> ColorModifier
		{
			get
			{
				var defaultColor = meshRenderer.material.color;
				return colorModifier ??= new Modifier<Color>(
					() => meshRenderer.material.color,
					value => meshRenderer.material.color = value,
					_ => defaultColor);
			}
		}

		public MeshRendererModifier(MeshRenderer meshRenderer)
		{
			this.meshRenderer = meshRenderer;
		}

		private Modifier<int>? sortingOrderModifier;

		private Modifier<Color>? colorModifier;
	}
}
