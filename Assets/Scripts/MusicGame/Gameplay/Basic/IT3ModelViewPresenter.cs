#nullable enable

using System.Collections.Generic;
using T3Framework.Preset.Wrapper;
using T3Framework.Runtime.Modifier;
using UnityEngine;

namespace MusicGame.Gameplay.Basic
{
	public interface IT3ModelViewPresenter
	{
		/// <summary> The color modifiers of what you think is the "main part" of this presenter. </summary>
		public IReadOnlyCollection<Modifier<Color>> ColorModifiers { get; }

		public RendererModifier MainTexture { get; }

		public IReadOnlyDictionary<string, RendererModifier> Textures { get; }
	}
}