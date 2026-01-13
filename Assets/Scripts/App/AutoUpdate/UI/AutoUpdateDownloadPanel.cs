#nullable enable

using System.ComponentModel;
using App.AutoUpdate.Model;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using TMPro;
using UnityEngine;

namespace App.AutoUpdate.UI
{
	public class AutoUpdateDownloadPanel : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private VersionStatusDataContainer versionStatusDataContainer = default!;
		[SerializeField] private AutoUpdateWebRequestHandler handler = default!;
		[SerializeField] private GameObject panelRoot = default!;
		[SerializeField] private TMP_Text descriptionText = default!;
		[SerializeField] private TMP_Text updateLogText = default!;
		[SerializeField] private TMP_Text updateDateText = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<VersionStatus, VersionStatusDataContainer>
				(versionStatusDataContainer, OnVersionStatusChanged)
		};

		// Private
		private VersionDescriptor? descriptor;

		// Event Handlers
		private void OnVersionStatusChanged(object sender, PropertyChangedEventArgs e)
		{
			bool downloadSilently = ISingletonSetting<AutoUpdateSetting>.Instance.DownloadSilently;
			bool notDownloaded = versionStatusDataContainer.Property.Value == VersionStatus.HasUpdateAndNotDownloaded;
			if (downloadSilently && notDownloaded) handler.BeginDownloadProcess().Forget();
			panelRoot.SetActive(!downloadSilently && notDownloaded);

			if (!notDownloaded) return;
			descriptor = versionStatusDataContainer.TargetVersionDescriptor;
			descriptionText.text = descriptor.Value.Description;
			updateLogText.text = string.Join('\n', descriptor.Value.UpdateLog);
			updateDateText.text = descriptor.Value.UpdateDate;
		}
	}
}