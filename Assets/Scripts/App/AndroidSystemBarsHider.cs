#nullable enable

using System.Collections;
using UnityEngine;

namespace App
{
	// Keeps the Android status bar and navigation bar hidden at runtime.
	// Unity 6 applies WindowInsetsController.hide() only once during player
	// initialization, so on some devices any touch reveals the bars and they
	// never hide again. This component re-applies the hide request regularly
	// and on every focus gain.
	public sealed class AndroidSystemBarsHider : MonoBehaviour
	{
		private const int BehaviorShowTransientBarsBySwipe = 2;

		private const int LegacyImmersiveStickyFlags =
			0x100 // SYSTEM_UI_FLAG_LAYOUT_STABLE
			| 0x200 // SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
			| 0x2 // SYSTEM_UI_FLAG_HIDE_NAVIGATION
			| 0x400 // SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN
			| 0x4 // SYSTEM_UI_FLAG_FULLSCREEN
			| 0x1000; // SYSTEM_UI_FLAG_IMMERSIVE_STICKY

		[SerializeField] private float retryInterval = 0.5f;

		private bool supported;

		private void Awake()
		{
			supported = Application.platform == RuntimePlatform.Android;
			DontDestroyOnLoad(gameObject);
		}

		private void OnEnable()
		{
			if (!supported) return;
			HideSystemBars();
			StartCoroutine(HideLoop());
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if (supported && hasFocus) HideSystemBars();
		}

		private IEnumerator HideLoop()
		{
			var wait = new WaitForSeconds(retryInterval);
			while (true)
			{
				yield return wait;
				HideSystemBars();
			}
		}

		private static void HideSystemBars()
		{
			using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			if (activity == null) return;
			bool useInsetsController = GetSdkInt() >= 30;
			// AndroidJavaObject must not leak out of this scope, so the runnable
			// re-fetches everything it needs on the UI thread.
			activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
			{
				using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				using var currentActivity = player.GetStatic<AndroidJavaObject>("currentActivity");
				if (currentActivity == null) return;
				using var window = currentActivity.Call<AndroidJavaObject>("getWindow");
				if (window == null) return;
				using var decorView = window.Call<AndroidJavaObject>("getDecorView");
				if (decorView == null) return;
				if (useInsetsController)
				{
					HideWithInsetsController(decorView);
				}
				else
				{
					decorView.Call("setSystemUiVisibility", LegacyImmersiveStickyFlags);
				}

				return;

				void HideWithInsetsController(AndroidJavaObject view)
				{
					using var controller = view.Call<AndroidJavaObject>("getWindowInsetsController");
					if (controller == null) return;
					using var insetsType = new AndroidJavaClass("android.view.WindowInsets$Type");
					int bars = insetsType.CallStatic<int>("statusBars")
					           | insetsType.CallStatic<int>("navigationBars");
					using var rootInsets = view.Call<AndroidJavaObject>("getRootWindowInsets");
					if (rootInsets == null || !rootInsets.Call<bool>("isVisible", bars)) return;
					controller.Call("setSystemBarsBehavior", BehaviorShowTransientBarsBySwipe);
					controller.Call("hide", bars);
				}
			}));

			return;

			int GetSdkInt()
			{
				using var version = new AndroidJavaClass("android.os.Build$VERSION");
				return version.GetStatic<int>("SDK_INT");
			}
		}
	}
}