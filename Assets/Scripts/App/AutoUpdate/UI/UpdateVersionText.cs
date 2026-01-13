#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using TMPro;
using UnityEngine;

namespace App.AutoUpdate.UI
{
	public class UpdateVersionText : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private VersionStatusDataContainer versionStatusDataContainer = default!;
		[SerializeField] private TMP_Text versionText = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<VersionStatus, VersionStatusDataContainer>(
				versionStatusDataContainer, (_, _) =>
				{
					var status = versionStatusDataContainer.Property.Value;
					if (status is not (VersionStatus.Latest or VersionStatus.NotChecked))
					{
						versionText.text = $"v{versionStatusDataContainer.TargetVersionDescriptor.Version}";
					}
				})
		};
	}
}