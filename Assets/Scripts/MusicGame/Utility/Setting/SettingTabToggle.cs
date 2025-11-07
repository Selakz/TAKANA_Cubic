#nullable enable

using System;
using System.ComponentModel;
using System.Reflection;
using T3Framework.Runtime.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.Utility.Setting
{
	// TODO: Refactor it into T3Framework to be more common to use.
	public class SettingTabToggle : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Toggle settingTabToggle = default!;
		[SerializeField] private TMP_Text settingTabLabel = default!;
		[SerializeField] private Transform movableArea = default!;
		[SerializeField] private Transform panelParent = default!;

		public Transform PanelParent
		{
			get => panelParent;
			set => panelParent = value;
		}

		public string SettingClassName
		{
			get => settingClassName;
			set
			{
				settingClassName = value;
				if (!string.IsNullOrEmpty(settingClassName))
				{
					var settingType = Type.GetType(settingClassName);
					if (settingType is null) return;
					var description = settingType.GetCustomAttribute<DescriptionAttribute>();
					settingTabLabel.text = description is null ? settingType.Name : description.Description;
					if (settingItemGenerator != null)
					{
						settingItemGenerator.SettingClassName = settingClassName;
						settingItemGenerator.Generate();
					}
				}
			}
		}

		// Private
		private static LazyPrefab settingPanelPrefab = default!;
		private string settingClassName = string.Empty;
		private SettingItemGenerator? settingItemGenerator;
		private bool lastIsOn = false;

		// Event Handlers
		private void OnSettingTabToggleValueChanged(bool value)
		{
			if (lastIsOn == value) return;
			lastIsOn = value;

			var slideLength = 50 * (value ? -1 : 1);
			movableArea.localPosition = new(
				movableArea.localPosition.x + slideLength, movableArea.localPosition.y, movableArea.localPosition.z);

			if (settingItemGenerator is null)
			{
				var go = settingPanelPrefab.Instantiate(panelParent);
				settingItemGenerator = go.GetComponent<SettingItemGenerator>();
				settingItemGenerator.SettingClassName = SettingClassName;
			}
			else
			{
				settingItemGenerator.gameObject.SetActive(value);
			}
		}

		// System Functions
		void Awake()
		{
			// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
			settingPanelPrefab ??=
				new LazyPrefab("Prefabs/EditorUI/Setting/SettingPanelContent", "SettingPanelContentPrefab_OnLoad");
		}

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