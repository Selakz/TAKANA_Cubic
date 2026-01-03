#nullable enable

using System.Collections.Generic;
using T3Framework.Preset.Wrapper;
using T3Framework.Runtime;
using UnityEngine;

namespace MusicGame.Gameplay.Basic
{
	public interface IT3ModelViewPresenter
	{
		public Modifier<Color> ColorModifier { get; }

		// TODO: "MainTexture" and Textures["main"] should not be two split modifiers
		public SpriteRendererModifier MainTexture { get; }

		public IReadOnlyDictionary<string, SpriteRendererModifier> Textures { get; }
	}
}