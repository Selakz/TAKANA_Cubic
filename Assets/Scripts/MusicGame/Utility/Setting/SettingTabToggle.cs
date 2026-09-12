#nullable enable

using T3Framework.Runtime.I18N;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.Utility.Setting
{
	public class SettingTabToggle : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Toggle settingTabToggle = default!;
		[SerializeField] private I18NTextBlock labelTextBlock = default!;
		[SerializeField] private Transform movableArea = default!;

		public Toggle Toggle => settingTabToggle;

		public I18NTextBlock LabelTextBlock => labelTextBlock;

		// Private
		private bool lastIsOn = false;

		// Event Handlers
		private void OnSettingTabToggleValueChanged(bool value)
		{
			if (lastIsOn == value) return;
			lastIsOn = value;

			var slideLength = 50 * (value ? -1 : 1);
			movableArea.localPosition = new(
				movableArea.localPosition.x + slideLength, movableArea.localPosition.y, movableArea.localPosition.z);
		}

		// System Functions
		void OnEnable()
		{
			settingTabToggle.onValueChanged.AddListener(OnSettingTabToggleValueChanged);
			if (settingTabToggle.transform.parent.TryGetComponent<ToggleGroup>(out var toggleGroup))
			{
				settingTabToggle.group = toggleGroup;
			}
		}

		void OnDisable()
		{
			settingTabToggle.onValueChanged.RemoveListener(OnSettingTabToggleValueChanged);
			settingTabToggle.group = null;
		}
	}
}
