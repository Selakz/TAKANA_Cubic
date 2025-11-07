#nullable enable

using TMPro;
using UnityEngine;

namespace MusicGame.Utility.AutoUpdate.UI
{
	public class AppVersionText : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private TMP_Text versionText = default!;

		// System Functions
		void Start()
		{
			versionText.text = $"v{Application.version}";
		}
	}
}