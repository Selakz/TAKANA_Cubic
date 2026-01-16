#nullable enable

using System.Collections.Generic;
using UnityEngine.InputSystem.EnhancedTouch;

namespace MusicGame.Gameplay.Judge
{
	public interface IInputProcessSystem
	{
		/// <param name="touches"> It's sorted as Began -> Moved/Stationary -> Ended/Canceled </param>
		public void ProcessInput(IReadOnlyList<Touch> touches);
	}
}