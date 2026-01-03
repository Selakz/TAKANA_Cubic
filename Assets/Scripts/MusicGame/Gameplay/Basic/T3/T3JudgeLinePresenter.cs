#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Preset.Wrapper;
using T3Framework.Runtime;
using T3Framework.Runtime.Serialization.Inspector;
using UnityEngine;

namespace MusicGame.Gameplay.Basic.T3
{
	public class T3JudgeLinePresenter : MonoBehaviour, IT3ModelViewPresenter
	{
		// Serializable and Public
		[SerializeField] private SpriteRenderer mainTexture = default!;
		[SerializeField] private InspectorDictionary<string, SpriteRendererModifier> textures = new();

		public Modifier<Color> ColorModifier => throw new NotImplementedException();

		public SpriteRendererModifier MainTexture => mainModifier ??= new(mainTexture);

		public IReadOnlyDictionary<string, SpriteRendererModifier> Textures => textures.Value;

		private SpriteRendererModifier? mainModifier;
	}
}