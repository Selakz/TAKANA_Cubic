#nullable enable

using System.ComponentModel;
using T3Framework.Preset.DataContainers;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;

namespace MusicGame.Utility.AutoUpdate.UI
{
	public class AutoUpdateInstallPanel : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private VersionStatusDataContainer versionStatusDataContainer = default!;
		[SerializeField] private BoolDataContainer isInConfirmStateDataContainer = default!;
		[SerializeField] private GameObject panelRoot = default!;
		[SerializeField] private GameObject installPanel = default!;
		[SerializeField] private GameObject confirmPanel = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<VersionStatus, VersionStatusDataContainer>
				(versionStatusDataContainer, OnVersionStatusChanged),
			new DataContainerRegistrar<bool, BoolDataContainer>
				(isInConfirmStateDataContainer, OnConfirmStateChanged)
		};

		private void OnVersionStatusChanged(object sender, PropertyChangedEventArgs e)
		{
			var hasDownloaded = versionStatusDataContainer.Property.Value == VersionStatus.HasUpdateAndHasDownloaded;
			panelRoot.SetActive(hasDownloaded);
			if (hasDownloaded) isInConfirmStateDataContainer.Property.Value = false;
		}

		// Event Handlers
		private void OnConfirmStateChanged(object sender, PropertyChangedEventArgs e)
		{
			var state = isInConfirmStateDataContainer.Property.Value;
			installPanel.SetActive(!state);
			confirmPanel.SetActive(state);
		}
	}
}