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
using VContainer;

namespace MusicGame.Gameplay.Basic.T3
{
	public class T3NoteViewPresenter : T3MonoBehaviour, INoteViewPresenter, IT3ModelViewPresenter
	{
		// Serializable and Public
		[SerializeField] private SpriteRenderer mainTexture = default!;
		[SerializeField] private InspectorDictionary<string, SpriteRendererModifier> textures = new();
		[SerializeField] private string[] widthTextures = default!;
		[SerializeField] private string[] heightTextures = default!;

		public SpriteRendererModifier MainTexture => textures.Value["main"];

		public IReadOnlyDictionary<string, SpriteRendererModifier> Textures => textures.Value;

		public IReadOnlyCollection<Modifier<Vector2>> WidthModifiers => widthModifiers ??=
			widthTextures.Select(name => textures.Value[name].SizeModifier).ToArray();

		public IReadOnlyCollection<Modifier<Vector2>> HeightModifiers => heightModifiers ??=
			heightTextures.Select(name => textures.Value[name].SizeModifier).ToArray();

		public Modifier<Vector2> PositionModifier { get; private set; } = default!;

		public Modifier<Color> ColorModifier { get; private set; } = default!;

		// Private
		private Color SpriteColor
		{
			get => MainTexture.Value.color;
			set => MainTexture.Value.color = value;
		}

		private Vector2 Position
		{
			get => transform.localPosition;
			set
			{
				Vector3 position = new(value.x, value.y, transform.localPosition.z);
				transform.localPosition = position;
			}
		}

		private Modifier<Vector2>[]? widthModifiers;
		private Modifier<Vector2>[]? heightModifiers;

		// System Functions
		[Inject]
		public void BeforeAwake()
		{
			PositionModifier = new Modifier<Vector2>(
				() => Position,
				position => Position = position,
				_ => new(0, ISingletonSetting<PlayfieldSetting>.Instance.UpperThreshold + 1));
			ColorModifier = new Modifier<Color>(
				() => SpriteColor,
				color => SpriteColor = color,
				_ => Color.white);
		}

		private void Update()
		{
			PositionModifier.Update();
			foreach (var modifier in Textures.Values)
			{
				modifier.SizeModifier.Update();
				modifier.ColorModifier.Update();
			}
		}
	}
}