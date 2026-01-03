#nullable enable

using TMPro;
using UnityEngine;

namespace MusicGame.Utility
{
	public class FpsText : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private TextMeshProUGUI fpsText = default!;

		// Private
		private float fpsTimer = 0f;
		private int fpsFrameCount = 0;

		// System Functions
		void Update()
		{
			fpsFrameCount++;
			if (IncAndTryWrap(ref fpsTimer, Time.deltaTime, 1f))
			{
				fpsText.text = fpsFrameCount.ToString();
				fpsFrameCount = 0;
			}
		}

		private static bool IncAndTryWrap(ref float value, float delta, float max)
		{
			value += delta;
			if (value <= max) return false;
			value -= max;
			return true;
		}
	}
}