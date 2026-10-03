#if UNITY_EDITOR

#nullable enable

using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace App.Editor
{
	/// <summary>
	/// Moves StreamingAssets folders that must not be shipped out of the project for the duration of a build and
	/// restores them afterwards. Platform folders are only hidden while building another platform; the excluded
	/// folder is hidden for every platform.
	/// </summary>
	public class SelectiveStreamingAssetPacker : IPreprocessBuildWithReport, IPostprocessBuildWithReport
	{
		public const string TemporaryStoragePath = @"D:\Temp"; // Must be in the same disk volume of your project
		public const string AndroidAssetsFolderName = "Android";
		public const string iOSAssetsFolderName = "iOS";
		public const string WindowsAssetsFolderName = "Windows";
		public const string ExcludedAssetsFolderName = "Excluded";

		public int callbackOrder => 0;

		public void OnPreprocessBuild(BuildReport report)
		{
			BuildTarget target = report.summary.platform;

			// Contents of the excluded folder are never shipped, whatever the target is.
			ToggleFolder(ExcludedAssetsFolderName, true);
			if (target is not BuildTarget.Android)
				ToggleFolder(AndroidAssetsFolderName, true);
			if (target is not BuildTarget.iOS)
				ToggleFolder(iOSAssetsFolderName, true);
			if (target is not (BuildTarget.StandaloneWindows or BuildTarget.StandaloneWindows64))
				ToggleFolder(WindowsAssetsFolderName, true);
			AssetDatabase.Refresh();
		}

		public void OnPostprocessBuild(BuildReport report)
		{
			ToggleFolder(AndroidAssetsFolderName, false);
			ToggleFolder(iOSAssetsFolderName, false);
			ToggleFolder(WindowsAssetsFolderName, false);
			ToggleFolder(ExcludedAssetsFolderName, false);
			AssetDatabase.Refresh();
		}

		private static void ToggleFolder(string folderName, bool hide)
		{
			string path = Path.Combine(Application.streamingAssetsPath, folderName);
			string hiddenPath = Path.Combine(TemporaryStoragePath, folderName);

			if (hide)
			{
				if (!Directory.Exists(path)) return;
				// A build that was interrupted before it restored the folder leaves a stale copy behind.
				if (Directory.Exists(hiddenPath))
				{
					Debug.LogWarning($"[Build] Replacing stale folder in temporary storage: {hiddenPath}");
					Directory.Delete(hiddenPath, true);
				}

				Directory.Move(path, hiddenPath);
				MoveMeta(path, hiddenPath);
				Debug.Log($"[Build] Exclude folder temporarily: {folderName}");
			}
			else
			{
				if (!Directory.Exists(hiddenPath)) return;
				if (Directory.Exists(path))
				{
					Debug.LogWarning($"[Build] Dropping stale folder in temporary storage: {hiddenPath}");
					Directory.Delete(hiddenPath, true);
				}
				else
				{
					Directory.Move(hiddenPath, path);
					MoveMeta(hiddenPath, path);
				}

				Debug.Log($"[Build] Restore folder: {folderName}");
			}

			static void MoveMeta(string fromFolder, string toFolder)
			{
				string fromMeta = fromFolder + ".meta";
				string toMeta = toFolder + ".meta";
				if (File.Exists(toMeta)) File.Delete(toMeta);
				if (File.Exists(fromMeta)) File.Move(fromMeta, toMeta);
			}
		}
	}
}

#endif
