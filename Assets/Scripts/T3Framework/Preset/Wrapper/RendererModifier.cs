#nullable enable

using T3Framework.Runtime.Modifier;
using UnityEngine;

namespace T3Framework.Preset.Wrapper
{
	public abstract class RendererModifier
	{
		public abstract Renderer Value { get; }

		public abstract Modifier<Color> ColorModifier { get; }

		public abstract Modifier<int> SortingOrderModifier { get; }
	}
}
