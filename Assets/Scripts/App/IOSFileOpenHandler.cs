#nullable enable

#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.IO;
#endif
using UnityEngine;

namespace App
{
	// Handles files sent to the app from the iOS share sheet. iOS copies the
	// incoming file into Documents/Inbox and reports it as a file:// deep link;
	// this handler moves it into the level storage folder consumed by the game.
	public sealed class IOSFileOpenHandler : MonoBehaviour
	{
		private static readonly string[] SupportedExtensions = { ".t3pkg", ".t3bundle" };

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Register()
		{
#if UNITY_IOS && !UNITY_EDITOR
			GameObject host = new("IOSFileOpenHandler");
			DontDestroyOnLoad(host);
			host.AddComponent<IOSFileOpenHandler>();
#endif
		}

		private void Awake()
		{
			Application.deepLinkActivated += OnDeepLinkActivated;
#if UNITY_IOS && !UNITY_EDITOR
			// When the app is cold-started by opening a file, the deep link event
			// may fire before this object exists, so check the launch URL too.
			if (!string.IsNullOrEmpty(Application.absoluteURL)) OnDeepLinkActivated(Application.absoluteURL);
#endif
		}

		private void OnDestroy()
		{
			Application.deepLinkActivated -= OnDeepLinkActivated;
		}

		private void OnDeepLinkActivated(string url)
		{
#if UNITY_IOS && !UNITY_EDITOR
			if (!url.StartsWith("file://", StringComparison.OrdinalIgnoreCase)) return;
			try
			{
				MoveIncomingFile(new Uri(url).LocalPath);
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to handle opened file {url}: {e.Message}");
			}

			return;

			void MoveIncomingFile(string filePath)
			{
				// Already moved by a previous callback, keep the handler idempotent.
				if (!File.Exists(filePath)) return;
				string extension = Path.GetExtension(filePath).ToLowerInvariant();
				if (Array.IndexOf(SupportedExtensions, extension) < 0) return;

				// Keep consistent with LevelSetting.StoragePath on iOS.
				string storagePath = Path.Combine(Application.persistentDataPath, "Levels");
				Directory.CreateDirectory(storagePath);
				string targetPath = Path.Combine(storagePath, Path.GetFileName(filePath));
				File.Move(filePath, targetPath, true);
				Debug.Log($"Moved opened file {filePath} to {targetPath}");
			}
#endif
		}
	}
}