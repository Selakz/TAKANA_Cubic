#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Wrapper;
using T3Framework.Runtime;
using T3Framework.Runtime.Modifier;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Static;
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

		public SpriteRendererModifier MainTexture => textures.Value["main"];

		public IReadOnlyDictionary<string, SpriteRendererModifier> Textures => textures.Value;

		public SpriteRenderer LeftLineTexture => leftLineTexture;

		public SpriteRenderer RightLineTexture => rightLineTexture;

		public Modifier<float> PositionModifier { get; private set; } = default!;

		public Modifier<float> WidthModifier { get; private set; } = default!;

		public IReadOnlyCollection<Modifier<Color>> ColorModifiers => colorModifiers ??=
			new[] { MainTexture.ColorModifier };

		// IT3ModelViewPresenter Explicit Implementation
		RendererModifier IT3ModelViewPresenter.MainTexture => MainTexture;

		IReadOnlyDictionary<string, RendererModifier> IT3ModelViewPresenter.Textures =>
			texturesAsBase ??= textures.Value.ToDictionary(
				kvp => kvp.Key, RendererModifier (kvp) => kvp.Value);

		// Private
		private IReadOnlyCollection<Modifier<Color>>? colorModifiers;
		private Dictionary<string, RendererModifier>? texturesAsBase;

		private float Width
		{
			get => mainTexture.size.x;
			set
			{
				mainTexture.size = new(value, mainTexture.size.y);
				leftLineTexture.transform.localPosition =
					leftLineTexture.transform.localPosition with { x = -value / 2 };
				rightLineTexture.transform.localPosition =
					rightLineTexture.transform.localPosition with { x = value / 2 };
			}
		}

		private float Position
		{
			get => transform.localPosition.x;
			set => transform.localPosition = new(value, 0, transform.localPosition.z);
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
			MainTexture.ColorModifier.Register(
				_ => ISingleton<PlayfieldSetting>.Instance.TrackFaceDefaultColor,
				-1);
			Textures["leftLine"].ColorModifier.Register(
				color => color with { a = ISingleton<PlayfieldSetting>.Instance.TrackFaceDefaultColor.Value.a },
				-1);
			Textures["rightLine"].ColorModifier.Register(
				color => color with { a = ISingleton<PlayfieldSetting>.Instance.TrackFaceDefaultColor.Value.a },
				-1);
		}

		private void Update()
		{
			PositionModifier.Update();
			WidthModifier.Update();
			foreach (var cm in ColorModifiers) cm.Update();
		}
	}
}