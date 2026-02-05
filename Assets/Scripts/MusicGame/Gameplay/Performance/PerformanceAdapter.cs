#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Static;
using UnityEngine;

namespace MusicGame.Gameplay.Performance
{
	public class PerformanceAdapter : T3MonoBehaviour
	{
		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(ISingleton<PerformanceSetting>.Instance.UseVSync,
				use => { QualitySettings.vSyncCount = use ? 1 : 0; }),
			new PropertyRegistrar<int>(ISingleton<PerformanceSetting>.Instance.TargetFrameRate,
				frameRate => { Application.targetFrameRate = frameRate; }),
#if UNITY_ANDROID || UNITY_IOS
			new PropertyRegistrar<float>(ISingleton<PerformanceSetting>.Instance.ResolutionRatio, ratio =>
			{
				screenWidth ??= Screen.width;
				screenHeight ??= Screen.height;
				var width = Mathf.RoundToInt(screenWidth.Value * ratio);
				var height = Mathf.RoundToInt(screenHeight.Value * ratio);
				Screen.SetResolution(width, height, Screen.fullScreen);
			})
#endif
		};

		// Static
		private static float? screenWidth;
		private static float? screenHeight;

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			DontDestroyOnLoad(gameObject);
		}
	}
}