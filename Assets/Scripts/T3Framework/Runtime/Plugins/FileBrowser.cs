using SFB;
using UnityEngine;
using UnityEngine.Networking;

namespace T3Framework.Runtime.Plugins
{
	public static class FileBrowser
	{
		public static string OpenFileDialog(string title, string initPath, params ExtensionFilter[] extensions)
		{
			string[] chosenPaths = StandaloneFileBrowser.OpenFilePanel(title, initPath, extensions, false);
			if (chosenPaths.Length > 0 && chosenPaths[0] != "")
			{
				string path = chosenPaths[0];
				if (path.StartsWith("file://"))
				{
					path = UnityWebRequest.UnEscapeURL(path.Replace("file://", ""));
				}

				return path;
			}
			else
			{
				return null;
			}
		}

		public static string OpenFolderDialog(string title = "", string initPath = "")
		{
			string[] chosenPaths = StandaloneFileBrowser.OpenFolderPanel(title, initPath, false);
			if (chosenPaths.Length > 0 && chosenPaths[0] != "")
			{
				string path = chosenPaths[0];
				if (path.StartsWith("file://"))
				{
					path = UnityWebRequest.UnEscapeURL(path.Replace("file://", ""));
				}

				return path;
			}
			else
			{
				return null;
			}
		}

		public static void OpenExplorer(string selectPath)
		{
			if (Application.platform == RuntimePlatform.WindowsEditor ||
			    Application.platform == RuntimePlatform.WindowsPlayer)
			{
				System.Diagnostics.Process.Start(selectPath.Replace("/", "\\"));
			}
			else
			{
				var p = new System.Diagnostics.Process();
				p.StartInfo.FileName =
					Application.platform is RuntimePlatform.LinuxEditor or RuntimePlatform.LinuxPlayer
						? "xdg-open"
						: "open";
				p.StartInfo.Arguments = "\"" + selectPath + "\"";
				p.StartInfo.UseShellExecute = false;
				p.Start();
				p.WaitForExit();
				p.Close();
			}
		}
	}
}