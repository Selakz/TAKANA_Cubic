#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using T3Framework.Preset.Wrapper;
using T3Framework.Runtime.Modifier;
using T3Framework.Runtime.Serialization.Inspector;
using UnityEngine;

namespace MusicGame.Gameplay.Basic.T3
{
	public class T3JudgeLinePresenter : MonoBehaviour, IT3ModelViewPresenter
	{
		// Serializable and Public
		[SerializeField] private SpriteRenderer mainTexture = default!;
		[SerializeField] private InspectorDictionary<string, SpriteRendererModifier> textures = new();

		public IReadOnlyCollection<Modifier<Color>> ColorModifiers => throw new NotImplementedException();

		public SpriteRendererModifier MainTexture => mainModifier ??= new(mainTexture);

		// IT3ModelViewPresenter Explicit Implementation
		RendererModifier IT3ModelViewPresenter.MainTexture => MainTexture;

		IReadOnlyDictionary<string, RendererModifier> IT3ModelViewPresenter.Textures =>
			texturesAsBase ??= textures.Value.ToDictionary(
				kvp => kvp.Key, RendererModifier (kvp) => kvp.Value);

		private SpriteRendererModifier? mainModifier;
		private Dictionary<string, RendererModifier>? texturesAsBase;
	}
}