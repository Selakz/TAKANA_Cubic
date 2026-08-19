#nullable enable

using System.Collections.Generic;
using EditorPlugin.PluginSystem;
using T3Framework.Runtime.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditorPlugin.EditorIntegration.UI
{
	public class EditPluginHotkeyContent : MonoBehaviour
	{
		[SerializeField] private GameObject paramContentPrefab = default!;
		[SerializeField] private int dropdownInputOptionsCount = 10;

		[field: SerializeField]
		public TMP_Text NameText { get; set; } = default!;

		[field: SerializeField]
		public Transform ParamsParent { get; set; } = default!; // Stores EditPluginParamContent

		[field: SerializeField]
		public TMP_Dropdown HotkeyDropdown { get; set; } = default!;

		[field: SerializeField]
		public Button DeleteButton { get; set; } = default!;

		public IReadOnlyDictionary<string, EditPluginParamContent> ParamContents => paramContents;

		// Private
		private readonly Dictionary<string, EditPluginParamContent> paramContents = new();
		private readonly List<EditPluginParamContent> paramContentList = new();

		public void UpdateParams(IReadOnlyList<PluginParam> @params)
		{
			EnsureCount(paramContentList, paramContentPrefab, ParamsParent, @params.Count);
			paramContents.Clear();
			for (int i = 0; i < @params.Count; i++)
			{
				var paramContent = paramContentList[i];
				paramContent.gameObject.SetActive(true);
				paramContent.NameText.text = @params[i].Name.Value;
				paramContent.UpdateUI(@params[i].Type, @params[i].Metas);
				paramContents[@params[i].Id] = paramContent;
			}

			for (int i = @params.Count; i < paramContentList.Count; i++)
			{
				paramContentList[i].gameObject.SetActive(false);
			}
		}

		private static void EnsureCount<T>(List<T> list, GameObject prefab, Transform parent, int count)
			where T : MonoBehaviour
		{
			for (int i = list.Count; i < count; i++)
			{
				list.Add(Instantiate(prefab, parent).GetComponent<T>());
			}
		}

		// System Functions
		void Awake()
		{
			List<int> options = new();
			for (int i = 0; i <= dropdownInputOptionsCount; i++) options.Add(i);
			HotkeyDropdown.SetOptions(options, option => option == 0 ? "None" : $"Hotkey {option}");
		}
	}
}