#if UNITY_EDITOR

#nullable enable

using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace App.Editor
{
	public class SelectiveStreamingAssetPacker : IPreprocessBuildWithReport, IPostprocessBuildWithReport
	{
		public const string TemporaryStoragePath = @"D:\Temp"; // Must be in the same disk volume of your project
		public const string AndroidAssetsFolderName = "Android";
		public const string iOSAssetsFolderName = "iOS";
		public const string WindowsAssetsFolderName = "Windows";

		public int callbackOrder => 0;

		public void OnPreprocessBuild(BuildReport report)
		{
			BuildTarget target = report.summary.platform;

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
			AssetDatabase.Refresh();
		}

		private static void ToggleFolder(string folderName, bool hide)
		{
			string path = Path.Combine(Application.streamingAssetsPath, folderName);
			string metaPath = path + ".meta";
			string hiddenPath = Path.Combine(TemporaryStoragePath, folderName);
			string hiddenMetaPath = hiddenPath + ".meta";

			if (hide && Directory.Exists(path))
			{
				Directory.Move(path, hiddenPath);
				File.Move(metaPath, hiddenMetaPath);
				Debug.Log($"[Build] Exclude folder temporarily: {folderName}");
			}
			else if (!hide && Directory.Exists(hiddenPath))
			{
				Directory.Move(hiddenPath, path);
				File.Move(hiddenMetaPath, metaPath);
				Debug.Log($"[Build] Restore folder: {folderName}");
			}
		}
	}
}

#endif