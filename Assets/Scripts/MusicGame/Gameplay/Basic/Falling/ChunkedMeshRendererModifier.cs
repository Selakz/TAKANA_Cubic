#nullable enable

using System.Collections.Generic;
using T3Framework.Preset.Wrapper;
using T3Framework.Runtime.Modifier;
using UnityEngine;

namespace MusicGame.Gameplay.Basic.Falling
{
	public sealed class ChunkedMeshRendererModifier : RendererModifier
	{
		private readonly List<MeshRenderer> renderers = new();

		private Modifier<Color>? colorModifier;

		private Modifier<int>? sortingOrderModifier;

		public override Renderer Value => renderers.Count > 0 ? renderers[0] : default!;

		public override Modifier<Color> ColorModifier => colorModifier ??= new(
			() => renderers.Count > 0 ? renderers[0].material.color : default,
			value =>
			{
				foreach (var r in renderers) r.material.color = value;
			},
			_ => Color.white);

		public override Modifier<int> SortingOrderModifier => sortingOrderModifier ??= new(
			() => renderers.Count > 0 ? renderers[0].sortingOrder : 0,
			value =>
			{
				foreach (var r in renderers) r.sortingOrder = value;
			},
			_ => 0);

		public void Attach(MeshRenderer renderer)
		{
			renderers.Add(renderer);
			ColorModifier.Update();
			SortingOrderModifier.Update();
		}

		public void Detach(MeshRenderer renderer) => renderers.Remove(renderer);

		public void DetachAll() => renderers.Clear();
	}
}