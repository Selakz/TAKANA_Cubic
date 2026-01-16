#nullable enable

using T3Framework.Runtime;
using UnityEngine.InputSystem.EnhancedTouch;

namespace MusicGame.Gameplay.Judge
{
	public class InputStorage : T3MonoBehaviour
	{
		// System Functions
		void Update()
		{
			foreach (var touch in Touch.activeTouches)
			{
			}
		}
	}
}