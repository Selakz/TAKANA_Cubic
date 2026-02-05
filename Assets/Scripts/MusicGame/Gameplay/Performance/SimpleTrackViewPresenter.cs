#nullable enable

using MusicGame.Gameplay.Basic;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Modifier;
using T3Framework.Runtime.Setting;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Performance
{
	public class SimpleTrackViewPresenter : T3MonoBehaviour, ITrackViewPresenter
	{
		// Serializable and Public
		[SerializeField] private SpriteRenderer mainTexture = default!;

		public Modifier<float> PositionModifier { get; private set; } = default!;

		public Modifier<float> WidthModifier { get; private set; } = default!;

		public Modifier<Color> ColorModifier { get; private set; } = default!;

		// Private
		private float Width
		{
			get => mainTexture.size.x;
			set => mainTexture.size = new(value, mainTexture.size.y);
		}

		private float Position
		{
			get => transform.localPosition.x;
			set => transform.localPosition = new(value, 0, transform.localPosition.z);
		}

		// Constructor
		[Inject]
		private void Construct()
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
	}
}