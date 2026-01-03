#nullable enable

using System.Collections.Generic;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Wrapper;
using T3Framework.Runtime;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.Setting;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Basic.T3
{
	public class T3TrackViewPresenter : T3MonoBehaviour, ITrackViewPresenter, IT3ModelViewPresenter
	{
		// Serializable and Public
		[SerializeField] private SpriteRenderer mainTexture = default!;
		[SerializeField] private SpriteRenderer leftLineTexture = default!;
		[SerializeField] private SpriteRenderer rightLineTexture = default!;
		[SerializeField] private InspectorDictionary<string, SpriteRendererModifier> textures = new();

		public SpriteRendererModifier MainTexture => mainModifier ??= new(mainTexture);

		public SpriteRenderer LeftLineTexture => leftLineTexture;

		public SpriteRenderer RightLineTexture => rightLineTexture;

		public IReadOnlyDictionary<string, SpriteRendererModifier> Textures => textures.Value;

		public Modifier<float> PositionModifier { get; private set; } = default!;

		public Modifier<float> WidthModifier { get; private set; } = default!;

		public Modifier<Color> ColorModifier { get; private set; } = default!;

		// Private
		private SpriteRendererModifier? mainModifier;

		private float Width
		{
			get => mainTexture.size.x;
			set
			{
				mainTexture.size = new(value, mainTexture.size.y);
				leftLineTexture.transform.localPosition = new(-value / 2, 0);
				rightLineTexture.transform.localPosition = new(value / 2, 0);
			}
		}

		private float Position
		{
			get => transform.localPosition.x;
			set => transform.localPosition = new(value, 0);
		}

		// System Functions
		[Inject]
		public void BeforeAwake()
		{
			PositionModifier = new Modifier<float>(
				() => Position,
				position => Position = position,
				_ => 0);
			WidthModifier = new Modifier<float>(
				() => Width,
				width => Width = width,
				_ => 1);
			ColorModifier = new Modifier<Color>(
				() => mainTexture.color,
				value => mainTexture.color = value,
				_ => ISingletonSetting<PlayfieldSetting>.Instance.TrackFaceDefaultColor);
		}

		private void Update()
		{
			PositionModifier.Update();
			WidthModifier.Update();
			ColorModifier.Update();
		}
	}
}