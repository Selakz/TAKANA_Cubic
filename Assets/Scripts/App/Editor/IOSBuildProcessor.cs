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
			// LSSupportsOpeningDocumentsInPlace: Must stay disabled, otherwise files
			// opened from the share sheet are not copied into Documents/Inbox but
			// opened in place, which the runtime handler cannot move safely.
			rootDict.SetBoolean("LSSupportsOpeningDocumentsInPlace", false);

			RegisterDocumentTypes();

			File.WriteAllText(plistPath, plist.WriteToString());
			Debug.Log("[XcodePostProcess] Configured file sharing and document types in Info.plist.");

			void RegisterDocumentTypes()
			{
				string utiPrefix = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.iOS);
				if (string.IsNullOrEmpty(utiPrefix)) utiPrefix = "com.takana.cubic";
				var types = new[]
				{
					(Extension: "t3pkg", TypeName: "TAKANA Cubic Level Package"),
					(Extension: "t3bundle", TypeName: "TAKANA Cubic Level Bundle"),
				};

				// Remove stale entries first so append-mode builds do not duplicate them.
				rootDict.values.Remove("CFBundleDocumentTypes");
				rootDict.values.Remove("UTExportedTypeDeclarations");
				PlistElementArray documentTypes = rootDict.CreateArray("CFBundleDocumentTypes");
				PlistElementArray exportedTypes = rootDict.CreateArray("UTExportedTypeDeclarations");
				foreach (var (extension, typeName) in types)
				{
					string typeIdentifier = $"{utiPrefix}.{extension}";

					PlistElementDict documentType = documentTypes.AddDict();
					documentType.SetString("CFBundleTypeName", typeName);
					documentType.SetString("CFBundleTypeRole", "Viewer");
					documentType.SetString("LSHandlerRank", "Owner");
					documentType.CreateArray("LSItemContentTypes").AddString(typeIdentifier);

					PlistElementDict exportedType = exportedTypes.AddDict();
					exportedType.SetString("UTTypeIdentifier", typeIdentifier);
					exportedType.SetString("UTTypeDescription", typeName);
					exportedType.CreateArray("UTTypeConformsTo").AddString("public.data");
					PlistElementDict tagSpecification = exportedType.CreateDict("UTTypeTagSpecification");
					tagSpecification.CreateArray("public.filename-extension").AddString(extension);
				}
			}
#endif
		}
	}
}

#endif