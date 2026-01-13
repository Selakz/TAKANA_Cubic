#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Semver;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace App.AutoUpdate
{
	/// <summary>
	/// It's a helper class about a helper.
	/// </summary>
	public static class AutoUpdateHelperHelper
	{
		private const SemVersionStyles VersionStyles = SemVersionStyles.AllowV | SemVersionStyles.OptionalPatch;

		private const string HelperFileName = "TakanaAutoUpdateHelper.exe";

		private const int TimeoutMilliseconds = 5000;

		public static string NewHelperFilePath =>
			Path.Combine(Application.streamingAssetsPath, "Windows", HelperFileName);

		public static string DefaultHelperFilePath =>
			Path.Combine(Application.persistentDataPath, "AutoUpdate", HelperFileName);

		public static string ResponseFilePath => Path.Combine(Application.persistentDataPath, "AutoUpdate", "response");

		private static async UniTask<string?> StartHelperTask(ProcessStartInfo startInfo, TimeSpan timeout)
		{
			TaskCompletionSource<string?> responseFoundSource = new();
			ResponseFileWatcher watcher = new(ResponseFilePath);
			try
			{
				if (!watcher.StartListening(response => responseFoundSource.TrySetResult(response)))
				{
					return null;
				}

				using Process? process = Process.Start(startInfo);
				if (process is null) return null;

				var completedTask = await Task.WhenAny(
					responseFoundSource.Task,
					Task.Delay(timeout)
				);

				if (completedTask == responseFoundSource.Task) return responseFoundSource.Task.Result;
				else
				{
					process.Kill();
					return null;
				}
			}
			catch
			{
				return null;
			}
			finally
			{
				watcher.StopListening();
				watcher.ClearStatusFile();
			}
		}

		/// <summary> Replace old helper with new helper. </summary>
		/// <returns> If false, there's exception and user cannot do auto update. </returns>
		public static async UniTask<bool> UpdateUpdateHelper(SemVersion targetVersion)
		{
			var directory = Path.GetDirectoryName(DefaultHelperFilePath);
			if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);

			var isDefaultValid = await ValidateUpdateHelper(DefaultHelperFilePath, targetVersion);
			if (isDefaultValid)
			{
				if (File.Exists(NewHelperFilePath)) File.Delete(NewHelperFilePath);
				return true;
			}
			else
			{
				var isNewValid = await ValidateUpdateHelper(NewHelperFilePath, targetVersion);
				if (isNewValid)
				{
					File.Move(NewHelperFilePath, DefaultHelperFilePath);
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		private static async UniTask<bool> ValidateUpdateHelper(string filePath, SemVersion targetVersion)
		{
			if (!File.Exists(filePath)) return false;

			var startInfo = new ProcessStartInfo
			{
				FileName = filePath,
				WorkingDirectory = Path.GetDirectoryName(DefaultHelperFilePath)!,
				Arguments = $"-v \"{ResponseFilePath}\"",
				UseShellExecute = false,
				CreateNoWindow = true
			};

			var response = await StartHelperTask(startInfo, TimeSpan.FromMilliseconds(TimeoutMilliseconds));
			if (response is null) Debug.Log("null response of version find");
			SemVersion.TryParse(response, VersionStyles, out var outVersion);
			return outVersion == targetVersion;
		}

		public enum UpdatePackResult
		{
			Ready,
			ExecutionError,
			InvalidVersion,
			InvalidPackage,
			ProjectFound
		}

		/// <summary> If <see cref="UpdatePackResult.Ready"/>, the caller of this function should do program exit to let helper do update. </summary>
		public static async UniTask<UpdatePackResult> UpdatePack(string packVersion, bool isForce)
		{
			var appDirectory = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])!;
			var startInfo = new ProcessStartInfo
			{
				FileName = DefaultHelperFilePath,
				WorkingDirectory = Path.GetDirectoryName(DefaultHelperFilePath)!,
				Arguments = $"{(isForce ? "-uf" : "-u")} \"{ResponseFilePath}\" {packVersion} \"{appDirectory}\"",
				UseShellExecute = false,
				CreateNoWindow = true,
				Verb = "runas"
			};

			var response = await StartHelperTask(startInfo, TimeSpan.FromMilliseconds(TimeoutMilliseconds));
			return response switch
			{
				"Ready" => UpdatePackResult.Ready,
				"ExecutionError" => UpdatePackResult.ExecutionError,
				"InvalidVersion" => UpdatePackResult.InvalidVersion,
				"InvalidPackage" => UpdatePackResult.InvalidPackage,
				"ProjectFound" => UpdatePackResult.ProjectFound,
				_ => UpdatePackResult.ExecutionError
			};
		}

		public enum SavePackResult
		{
			Success,
			ExecutionError,
			InvalidPackage
		}

		public static async UniTask<SavePackResult> SavePack(string packVersion, string packPath)
		{
			var startInfo = new ProcessStartInfo
			{
				FileName = DefaultHelperFilePath,
				WorkingDirectory = Path.GetDirectoryName(DefaultHelperFilePath)!,
				Arguments = $"-s \"{ResponseFilePath}\" {packVersion} \"{packPath}\"",
				UseShellExecute = false,
				CreateNoWindow = true
			};

			var response = await StartHelperTask(startInfo, TimeSpan.FromMilliseconds(TimeoutMilliseconds));
			return response switch
			{
				"Success" => SavePackResult.Success,
				"ExecutionError" => SavePackResult.ExecutionError,
				"InvalidPackage" => SavePackResult.InvalidPackage,
				_ => SavePackResult.ExecutionError
			};
		}

		public enum CheckPackResult
		{
			Valid,
			Invalid
		}

		public static async UniTask<CheckPackResult> CheckPack(string packVersion)
		{
			var startInfo = new ProcessStartInfo
			{
				FileName = DefaultHelperFilePath,
				WorkingDirectory = Path.GetDirectoryName(DefaultHelperFilePath)!,
				Arguments = $"-c \"{ResponseFilePath}\" {packVersion}",
				UseShellExecute = false,
				CreateNoWindow = true
			};

			var response = await StartHelperTask(startInfo, TimeSpan.FromMilliseconds(TimeoutMilliseconds));
			return response switch
			{
				"Valid" => CheckPackResult.Valid,
				"Invalid" => CheckPackResult.Invalid,
				_ => CheckPackResult.Invalid
			};
		}
	}
}