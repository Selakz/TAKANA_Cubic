#nullable enable

using System;
using Cysharp.Threading.Tasks;
using T3Framework.Runtime;
using T3Framework.Runtime.Log;
using UnityEngine;

namespace App.AutoUpdate
{
	public class AutoUpdateWebRequestHandler : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private VersionStatusDataContainer versionStatusDataContainer = default!;
		[SerializeField] private DownloadProgressDataContainer downloadProgressDataContainer = default!;
		[SerializeField] private AutoUpdateWebRequestHelper webRequest = default!;
		[SerializeField] private AutoUpdateHelperHandler handler = default!;

		/// <returns> If having update, true</returns>
		public async UniTask<bool> BeginCheckUpdateProcess()
		{
			var descriptor = await webRequest.CheckForUpdateAsync();
			if (descriptor is null)
			{
				versionStatusDataContainer.Property.Value = VersionStatus.Latest;
				return false;
			}
			else
			{
				var currentDescriptor = versionStatusDataContainer.TargetVersionDescriptor;
				var currentStatus = versionStatusDataContainer.Property.Value;
				var nextStatus = currentStatus switch
				{
					VersionStatus.NotChecked or VersionStatus.Latest or VersionStatus.HasUpdateAndNotDownloaded
						=> VersionStatus.HasUpdateAndNotDownloaded,
					VersionStatus.HasUpdateAndIsDownloading
						=> VersionStatus.HasUpdateAndIsDownloading,
					VersionStatus.HasUpdateAndHasDownloaded
						=> descriptor.Value.Version == currentDescriptor.Version
							? VersionStatus.HasUpdateAndHasDownloaded
							: VersionStatus.HasUpdateAndNotDownloaded,
					_ => throw new ArgumentOutOfRangeException()
				};
				versionStatusDataContainer.TargetVersionDescriptor = descriptor.Value;
				versionStatusDataContainer.Property.Value = nextStatus;
				versionStatusDataContainer.Property.AddUpNotify();
				return true;
			}
		}

		public void BeginInstallProcess(bool isForce)
		{
			var status = versionStatusDataContainer.Property.Value;
			if (status != VersionStatus.HasUpdateAndHasDownloaded) return;
			var descriptor = versionStatusDataContainer.TargetVersionDescriptor;
			handler.TryInstall(descriptor.Version, isForce).Forget();
		}

		public async UniTaskVoid BeginDownloadProcess()
		{
			var status = versionStatusDataContainer.Property.Value;
			if (status is not VersionStatus.HasUpdateAndNotDownloaded)
			{
				versionStatusDataContainer.Property.ForceNotify();
				return;
			}

			var version = versionStatusDataContainer.TargetVersionDescriptor.Version;
			if (await handler.IsPackValid(version))
			{
				versionStatusDataContainer.Property.Value = VersionStatus.HasUpdateAndHasDownloaded;
				return;
			}

			versionStatusDataContainer.Property.Value = VersionStatus.HasUpdateAndIsDownloading;
			var packPath = await webRequest.DownloadPackAsync(version, OnProgress);
			if (string.IsNullOrEmpty(packPath))
			{
				versionStatusDataContainer.Property.Value = VersionStatus.HasUpdateAndNotDownloaded;
				T3Logger.Log("Notice", "AutoUpdate_DownloadFail", T3LogType.Warn);
			}
			else
			{
				handler.TrySavePack(version, packPath!).Forget();
			}
		}

		private void OnProgress(long totalBytes, long downloadedBytes, float progressPercentage)
		{
			downloadProgressDataContainer.Property.Value = new(totalBytes, downloadedBytes, progressPercentage);
		}
	}
}