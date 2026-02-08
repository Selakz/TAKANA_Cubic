#nullable enable

using T3Framework.Runtime.Modifier;
using UnityEngine;

namespace MusicGame.Gameplay.Basic
{
	public interface INoteViewPresenter
	{
		public Modifier<Vector2> PositionModifier { get; }
	}
}