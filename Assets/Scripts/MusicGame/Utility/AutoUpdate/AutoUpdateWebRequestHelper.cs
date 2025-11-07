#nullable enable

using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using MusicGame.Utility.AutoUpdate.Model;
using MusicGame.Utility.AutoUpdate.Schema;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.WebRequest;
using UnityEngine;

namespace MusicGame.Utility.AutoUpdate
{
	public class AutoUpdateWebRequestHelper : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private AutoUpdateConfig config = default!;

		// public static string DefaultSaveDirectory => Application.streamingAssetsPath;
		public static string DefaultSaveDirectory => "G:\\";

		// Private
		private CancellationTokenSource? downloadTokenSource;

		// Defined Functions
		/// <returns> The descriptor of the latest version if having update else null. </returns>
		public async UniTask<VersionDescriptor?> CheckForUpdateAsync()
		{
			var response = await WebRequestManager.Instance
				.GetWithStructureResponseAsync<CheckUpdateQuery, CheckUpdateResponse>
					(config.updateUrl, new() { CurrentVersion = Application.version });
			return response?.VersionDescriptor;
		}

		public async UniTask<VersionDescriptor?> GetVersionDescriptorAsync(string version)
		{
			var response = await WebRequestManager.Instance
				.GetWithStructureResponseAsync<VersionQuery, VersionResponse>
					(config.versionUrl, new() { Version = version });
			return response?.VersionDescriptor;
		}

		public UniTask<VersionsResponse?> GetVersionDescriptorsAsync(VersionsQuery query)
		{
			return WebRequestManager.Instance
				.GetWithStructureResponseAsync<VersionsQuery, VersionsResponse>(config.versionsUrl, query);
		}

		/// <returns> The path of the downloaded file if successful else null. </returns>
		public async UniTask<string?> DownloadPackAsync(string version, ProgressHandler? onProgress = null)
		{
			if (downloadTokenSource is not null)
			{
				downloadTokenSource.Cancel();
				downloadTokenSource.Dispose();
			}

			downloadTokenSource = new CancellationTokenSource();
			await UniTask.DelayFrame(1);
			var savePath = Path.Combine(DefaultSaveDirectory, $"{version}.zip");
			var isSuccess = await WebRequestManager.Instance.GetWithFileResponseAndSaveAsync(
				config.downloadUrl,
				new DownloadPackQuery { Version = version },
				savePath,
				onProgress,
				downloadTokenSource.Token);
			if (!isSuccess && File.Exists(savePath)) File.Delete(savePath);
			return isSuccess ? savePath : null;
		}
	}
}