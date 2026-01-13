#nullable enable

using Semver;
using TMPro;
using UnityEngine;

namespace App.AutoUpdate.UI
{
	public class AppVersionText : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private TMP_Text versionText = default!;
		[Header("Optional")] [SerializeField] private TMP_Text? prereleaseText;

		// System Functions
		void Start()
		{
			if (SemVersion.TryParse(Application.version, out var version))
			{
				versionText.text = $"v{version.WithoutPrerelease()}";
				if (prereleaseText != null)
				{
					prereleaseText.text =
						$"{(string.IsNullOrEmpty(version.Prerelease) ? string.Empty : "-")}{version.Prerelease}";
				}
			}
		}
	}
}