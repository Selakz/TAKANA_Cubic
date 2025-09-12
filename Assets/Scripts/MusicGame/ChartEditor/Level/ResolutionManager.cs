#nullable enable

using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.Level
{
	public class ResolutionManager : MonoBehaviour
	{
		// Serializable and Public
		public static ResolutionManager? Instance { get; private set; }

		// Static
		private const int MinimumWindowWidth = 800;

		// Defined Functions
		public void SetWindowResolution(int width)
		{
			width = Mathf.Max(MinimumWindowWidth, width);
			var height = width * 9 / 16;
			Screen.SetResolution(width, height, false);
			ISingletonSetting<EditorSetting>.Instance.WindowWidth = width;
			ISingletonSetting<EditorSetting>.SaveInstance();
		}

		// System Functions
		void OnEnable()
		{
			Instance = this;
		}

#if !UNITY_EDITOR
		void Start()
		{
			var width = Mathf.Max(MinimumWindowWidth, ISingletonSetting<EditorSetting>.Instance.WindowWidth);
			var height = width * 9 / 16;
			Screen.SetResolution(width, height, false);
		}
#endif
	}
}