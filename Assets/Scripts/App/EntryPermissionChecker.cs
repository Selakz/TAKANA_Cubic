#nullable enable

using UnityEngine;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace App
{
	public class EntryPermissionChecker : MonoBehaviour
	{
		void Awake()
		{
#if UNITY_ANDROID
			CheckAndroidPermissions();
#endif
		}

#if UNITY_ANDROID
		private void CheckAndroidPermissions()
		{
			if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
			{
				Permission.RequestUserPermission(Permission.ExternalStorageRead);
			}

			if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
			{
				Permission.RequestUserPermission(Permission.ExternalStorageWrite);
			}

			using var buildVersion = new AndroidJavaClass("android.os.Build$VERSION");
			int sdkInt = buildVersion.GetStatic<int>("SDK_INT");
			if (sdkInt >= 30)
			{
				using var environment = new AndroidJavaClass("android.os.Environment");
				bool isExternalStorageManager = environment.CallStatic<bool>("isExternalStorageManager");
				if (!isExternalStorageManager) OpenSettings();
			}
		}

		private void OpenSettings()
		{
			try
			{
				using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
				string packageName = currentActivity.Call<string>("getPackageName");

				using var uriClass = new AndroidJavaClass("android.net.Uri");
				using var uri = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null);
				using var intent = new AndroidJavaObject(
					"android.content.Intent", "android.settings.MANAGE_APP_ALL_FILES_ACCESS_PERMISSION", uri);
				currentActivity.Call("startActivity", intent);
			}
			catch (System.Exception e)
			{
				Debug.LogError("Fails to jump to permission authorization page: " + e.Message);
				OpenGeneralAllFilesSettings();
			}
		}

		private void OpenGeneralAllFilesSettings()
		{
			using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			using var intent = new AndroidJavaObject(
				"android.content.Intent", "android.settings.MANAGE_ALL_FILES_ACCESS_PERMISSION");
			currentActivity.Call("startActivity", intent);
		}
#endif
	}
}