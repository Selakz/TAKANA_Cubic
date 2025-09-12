#nullable enable

using T3Framework.Runtime;
using UnityEngine;

namespace MusicGame.Components
{
	/// <summary> Generic view layer modifiers </summary>
	public interface IModifiableView2D
	{
		public Modifier<Vector2> PositionModifier { get; }

		public Modifier<Vector2> ScaleModifier { get; }

		public Modifier<float> RotationModifier { get; }

		public Modifier<Sprite> SpriteModifier { get; }

		public Modifier<Color> ColorModifier { get; }

		public Modifier<int> SortingOrderModifier { get; }
	}
}