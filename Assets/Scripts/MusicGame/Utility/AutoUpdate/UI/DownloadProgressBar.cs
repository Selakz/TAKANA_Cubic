#nullable enable

using System.ComponentModel;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.Utility.AutoUpdate.UI
{
	public class DownloadProgressBar : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private VersionStatusDataContainer versionStatusDataContainer = default!;
		[SerializeField] private DownloadProgressDataContainer downloadProgressDataContainer = default!;
		[SerializeField] private GameObject progressPanel = default!;
		[SerializeField] private Slider progressBar = default!;
		[SerializeField] private TMP_Text progressText = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<VersionStatus, VersionStatusDataContainer>
				(versionStatusDataContainer, OnVersionStatusChanged),
			new DataContainerRegistrar<DownloadProgress, DownloadProgressDataContainer>
				(downloadProgressDataContainer, OnDownloadProgressChanged)
		};

		// Event Handlers
		private void OnVersionStatusChanged(object sender, PropertyChangedEventArgs e)
		{
			bool isDownloading = versionStatusDataContainer.Property.Value is VersionStatus.HasUpdateAndIsDownloading;
			progressPanel.SetActive(isDownloading);

			if (!isDownloading) return;
			var descriptor = versionStatusDataContainer.TargetVersionDescriptor;
			T3Logger.Log("Notice", $"AutoUpdate_DownloadStart|{descriptor.Version}", T3LogType.Info);
		}

		private void OnDownloadProgressChanged(object sender, PropertyChangedEventArgs e)
		{
			var progress = downloadProgressDataContainer.Property.Value;
			progressBar.value = progressBar.maxValue * progress.ProgressPercentage;
			progressText.text =
				$"{GetByteSizeDescription(progress.DownloadedBytes)} / {GetByteSizeDescription(progress.TotalBytes)}";
		}

		private string GetByteSizeDescription(long bytes)
		{
			return bytes switch
			{
				< 1024 => $"{bytes} B",
				< 1024 * 1024 => $"{bytes / 1024f:0.00} KB",
				< 1024 * 1024 * 1024 => $"{bytes / 1024f / 1024f:0.00} MB",
				_ => $"{bytes / 1024f / 1024f / 1024f:0.00} GB"
			};
		}
	}
}