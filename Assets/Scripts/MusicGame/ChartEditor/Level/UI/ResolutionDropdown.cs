using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Setting;
using TMPro;
using UnityEngine;

namespace MusicGame.ChartEditor.Level.UI
{
	public class ResolutionDropdown : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private TMP_Dropdown resolutionDropdown;

		// Private
		private int[] windowWidths;

		// Static

		// Defined Functions
		public void OnResolutionDropdownValueChanged(int choice)
		{
			var width = windowWidths[choice];
			ResolutionManager.Instance!.SetWindowResolution(width);
		}

		// System Functions
		private void Awake()
		{
			resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownValueChanged);
			windowWidths = resolutionDropdown.SetOptions(
				ISingletonSetting<EditorSetting>.Instance.WindowWidthChoices.Value,
				width => $"{width}x{width * 9 / 16}");
			for (int i = 0; i < windowWidths.Length; i++)
			{
				if (windowWidths[i] == ISingletonSetting<EditorSetting>.Instance.WindowWidth)
				{
					resolutionDropdown.SetValueWithoutNotify(i);
				}
			}
		}
	}
}