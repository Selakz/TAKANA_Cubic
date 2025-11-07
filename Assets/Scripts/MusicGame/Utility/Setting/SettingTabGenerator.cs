#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.ListRender;
using UnityEngine;

namespace MusicGame.Utility.Setting
{
	public class SettingTabGenerator : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private bool generateOnStart = true;
		[SerializeField] private ListRendererInt settingTabRenderer = default!;
		[SerializeField] private List<string> settingClassNames = new();
		[SerializeField] private Transform panelParent = default!;

		// Defined Functions
		public void Generate()
		{
			for (int i = 0; i < settingClassNames.Count; i++)
			{
				var settingType = Type.GetType(settingClassNames[i]);
				if (settingType is null)
				{
					Debug.LogWarning($"Could not find class of type {settingClassNames[i]}");
					continue;
				}

				// 1. Check if the type is ISingletonSetting
				var singletonInterface = settingType.GetInterface("ISingletonSetting`1");
				if (singletonInterface is null)
				{
					Debug.LogWarning($"{settingClassNames[i]} is not a singleton setting class");
					continue;
				}

				var genericArgs = singletonInterface.GetGenericArguments();
				if (genericArgs[0] != settingType)
				{
					Debug.LogWarning($"{settingClassNames[i]} falsely implement ISingletonSetting<{genericArgs[0]}>");
					continue;
				}

				// 2. Generate it
				var toggle = settingTabRenderer.Add<SettingTabToggle>(i);
				toggle.SettingClassName = settingClassNames[i];
			}
		}

		// System Functions
		void Awake()
		{
			Dictionary<Type, LazyPrefab> listPrefabs = new()
			{
				[typeof(SettingTabToggle)] =
					new LazyPrefab("Prefabs/EditorUI/Setting/SettingTabToggle", "SettingTabTogglePrefab_OnLoad",
						go => go.GetComponent<SettingTabToggle>().PanelParent = panelParent)
			};
			settingTabRenderer.Init(listPrefabs);
		}

		void Start()
		{
			if (generateOnStart) Generate();
		}
	}
}