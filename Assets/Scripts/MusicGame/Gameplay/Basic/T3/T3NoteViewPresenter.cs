#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Wrapper;
using T3Framework.Runtime;
using T3Framework.Runtime.Modifier;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.Gameplay.Basic.T3
{
	public class T3NoteViewPresenter : T3MonoBehaviour, INoteViewPresenter, IT3ModelViewPresenter
	{
		// Serializable and Public
		[SerializeField] private SpriteRenderer mainTexture = default!;
		[SerializeField] private InspectorDictionary<string, SpriteRendererModifier> textures = new();
		[SerializeField] private string[] widthTextures = default!;
		[SerializeField] private string[] heightTextures = default!;
		[SerializeField] private string[] thicknessTextures = default!;
		[SerializeField] private string endTexture = default!;

		public SpriteRendererModifier MainTexture => textures.Value["main"];

		public Dictionary<string, SpriteRendererModifier> Textures => textures.Value;

		public IReadOnlyCollection<Modifier<Vector2>> WidthModifiers => widthModifiers ??=
			widthTextures.Select(name => textures.Value[name].SizeModifier).ToArray();

		public IReadOnlyCollection<Modifier<Vector2>> HeightModifiers
		{
			get
			{
				if (heightModifiers is not null) return heightModifiers;
				var modifiers = heightTextures.Select(name => textures.Value[name].SizeModifier);
				if (!string.IsNullOrEmpty(endTexture) && textures.Value.TryGetValue(endTexture, out var endModifier))
				{
					EndPosModifier = new Modifier<Vector2>(
						() => endModifier.Value.transform.localPosition,
						size => endModifier.Value.transform.localPosition = size,
						_ => endModifier.Value.transform.localPosition);
					modifiers = modifiers.Append(EndPosModifier);
				}

				heightModifiers = modifiers.ToArray();
				return heightModifiers;
			}
		}

		public IReadOnlyCollection<Modifier<Vector2>> ThicknessModifiers => thicknessModifiers ??=
			thicknessTextures.Select(name => textures.Value[name].SizeModifier).ToArray();

		public Modifier<Vector2>? EndPosModifier { get; private set; }

		public Modifier<Vector2> PositionModifier =>
			positionModifier ??= new Modifier<Vector2>(
				() => Position,
				position => Position = position,
				_ => new(0, ISingletonSetting<PlayfieldSetting>.Instance.UpperThreshold + 1));

		public Modifier<Color>[] ColorModifiers =>
			colorModifiers ??= textures.Value.Values.Select(t => t.ColorModifier).ToArray();

		// IT3ModelViewPresenter Explicit Implementation
		RendererModifier IT3ModelViewPresenter.MainTexture => MainTexture;

		IReadOnlyDictionary<string, RendererModifier> IT3ModelViewPresenter.Textures =>
			texturesAsBase ??= textures.Value.ToDictionary(
				kvp => kvp.Key, RendererModifier (kvp) => kvp.Value);

		// Private
		private Vector2 Position
		{
			get => transform.localPosition;
			set
			{
				Vector3 position = new(value.x, value.y, transform.localPosition.z);
				transform.localPosition = position;
			}
		}

		private Modifier<Vector2>? positionModifier;
		private Modifier<Color>[]? colorModifiers;
		private Modifier<Vector2>[]? widthModifiers;
		private Modifier<Vector2>[]? heightModifiers;
		private Modifier<Vector2>[]? thicknessModifiers;
		private Dictionary<string, RendererModifier>? texturesAsBase;
	}
}