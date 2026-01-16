#if UNITY_EDITOR

#nullable enable

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

#if UNITY_IOS
using System.IO;
using UnityEditor.iOS.Xcode;
using UnityEngine;
#endif

namespace App.Editor
{
	public class IOSBuildProcessor : IPostprocessBuildWithReport
	{
		public int callbackOrder => 1;

		public void OnPostprocessBuild(BuildReport report)
		{
			if (report.summary.platform != BuildTarget.iOS) return;

#if UNITY_IOS
			var pathToBuiltProject = report.summary.outputPath;
			string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
			PlistDocument plist = new PlistDocument();
			plist.ReadFromFile(plistPath);

			PlistElementDict rootDict = plist.root;
			// UIFileSharingEnabled: Allow app to show its Document folder in "File" app
			rootDict.SetBoolean("UIFileSharingEnabled", true);
			// LSSupportsOpeningDocumentsInPlace: Allow app to open and modify its files
			rootDict.SetBoolean("LSSupportsOpeningDocumentsInPlace", true);

			File.WriteAllText(plistPath, plist.WriteToString());
			Debug.Log("[XcodePostProcess] Open file sharing permission in Info.plist.");
#endif
		}
	}
}

#endif