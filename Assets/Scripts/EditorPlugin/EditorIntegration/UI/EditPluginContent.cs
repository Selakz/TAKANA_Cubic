#nullable enable

using System.Collections.Generic;
using EditorPlugin.PluginSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditorPlugin.EditorIntegration.UI
{
	public class EditPluginContent : MonoBehaviour
	{
		[SerializeField] private GameObject paramContentPrefab = default!;
		[SerializeField] private GameObject hotkeyContentPrefab = default!;

		[field: SerializeField]
		public TMP_Text NameText { get; set; } = default!;

		[field: SerializeField]
		public Button DetailButton { get; set; } = default!;

		[field: SerializeField]
		public Transform ParamsParent { get; set; } = default!; // Stores EditPluginParamContent

		[field: SerializeField]
		public Transform HotkeyContentParent { get; set; } = default!; // Stores EditPluginHotkeyContent

		[field: SerializeField]
		public Button CreateNewHotkeyButton { get; set; } = default!;

		[field: SerializeField]
		public Button ExecuteButton { get; set; } = default!;

		[field: SerializeField]
		public Button DeleteButton { get; set; } = default!;

		public IReadOnlyDictionary<string, EditPluginParamContent> ParamContents => paramContents;

		public IReadOnlyDictionary<HotkeyPreset, EditPluginHotkeyContent> HotkeyContents => hotkeyContents;

		// Private
		private readonly Dictionary<string, EditPluginParamContent> paramContents = new();
		private readonly Dictionary<HotkeyPreset, EditPluginHotkeyContent> hotkeyContents = new();
		private readonly List<EditPluginParamContent> paramContentList = new();
		private readonly List<EditPluginHotkeyContent> hotkeyContentList = new();

		public void UpdateParams(IReadOnlyList<PluginParam> @params, IReadOnlyList<HotkeyPreset> hotKeyPresets)
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

			EnsureCount(hotkeyContentList, hotkeyContentPrefab, HotkeyContentParent, hotKeyPresets.Count);
			hotkeyContents.Clear();
			for (int i = 0; i < hotKeyPresets.Count; i++)
			{
				var hotKeyContent = hotkeyContentList[i];
				hotKeyContent.gameObject.SetActive(true);
				hotKeyContent.UpdateParams(@params);
				hotkeyContents[hotKeyPresets[i]] = hotKeyContent;
			}

			for (int i = hotKeyPresets.Count; i < hotkeyContentList.Count; i++)
			{
				hotkeyContentList[i].gameObject.SetActive(false);
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
	}
}