#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_ANDROID && !UNITY_EDITOR
using System.Linq;
#endif

namespace T3Framework.Runtime.Extensions
{
	public static class FileHelper
	{
		public static string GetAbsolutePathFromRelative(string absoluteBasePath, string relativePath)
		{
			var baseDirectory = Path.GetDirectoryName(absoluteBasePath);
			if (baseDirectory is null)
				throw new ArgumentException("The absoluteBasePath is not a valid file path.", nameof(absoluteBasePath));

			Uri baseUri = new Uri(baseDirectory + Path.DirectorySeparatorChar);
			Uri relativeUri = new Uri(relativePath, UriKind.Relative);
			Uri targetUri = new Uri(baseUri, relativeUri);
			return targetUri.LocalPath;
		}

		/// <summary> Cross-platform text reader. </summary>
		public static async Task<string?> ReadTextAsync(string filePath)
		{
			if (filePath.Contains("://") || filePath.Contains(":///"))
			{
				using UnityWebRequest uwr = UnityWebRequest.Get(filePath);
				var operation = uwr.SendWebRequest();
				while (!operation.isDone) await Task.Yield();

				if (uwr.result == UnityWebRequest.Result.Success)
				{
					return uwr.downloadHandler.text;
				}
				else
				{
					Debug.LogError($"Read Text Fail: {filePath}, Error: {uwr.error}");
					return null;
				}
			}
			else
			{
				if (File.Exists(filePath))
				{
					return await File.ReadAllTextAsync(filePath);
				}
			}

			return null;
		}

		/// <summary> Cross-platform file fetcher. </summary>
		public static string[] GetFiles(string folderPath)
		{
			List<string> fileList = new List<string>();

#if UNITY_ANDROID && !UNITY_EDITOR
			if (folderPath.Contains("jar:file://") || folderPath.Contains(".apk!/assets"))
			{
				return GetAndroidStreamingAssetsFiles(folderPath);
			}
#endif

			if (Directory.Exists(folderPath))
			{
				string[] files = Directory.GetFiles(folderPath);
				fileList.AddRange(files);
			}
			else
			{
				Debug.LogWarning($"Path not exist: {folderPath}");
			}

			return fileList.ToArray();
		}

#if UNITY_ANDROID && !UNITY_EDITOR
		private static string[] GetAndroidStreamingAssetsFiles(string folderPath)
		{
			List<string> results = new List<string>();

			string relativePath = string.Empty;
			const string assetsTag = "!/assets/";
			int index = folderPath.IndexOf(assetsTag, StringComparison.Ordinal);
			if (index != -1)
			{
				relativePath = folderPath[(index + assetsTag.Length)..];
			}

			try
			{
				using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				using AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
				using AndroidJavaObject assetManager = currentActivity.Call<AndroidJavaObject>("getAssets");

				string[] files = assetManager.Call<string[]>("list", relativePath);
				results.AddRange(files.Select(
					fileName => Path.Combine(Application.streamingAssetsPath, relativePath, fileName)));
			}
			catch (Exception e)
			{
				Debug.LogError($"Android AssetManager Read Fail: {e.Message}");
			}

			return results.ToArray();
		}
#endif
	}
}