#if UNITY_EDITOR

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace T3Framework.Runtime.I18N.Editor
{
	public struct TextInfo
	{
		public string Path { get; set; }
		public string Text { get; set; }
		public string Key { get; set; }
	}

	public class SceneI18NHelper : EditorWindow
	{
		private const string JsonPath = "Assets/Resources/tmp_localization.json";

		[MenuItem("Tools/Localization/Export unlocalized texts to json")]
		public static void ExportTMPTextsToJson()
		{
			var texts = new List<TextInfo>();
			var tmpComponents = FindObjectsOfType<TextMeshProUGUI>(true);
			foreach (var tmp in tmpComponents)
			{
				if (string.IsNullOrEmpty(tmp.text) || float.TryParse(tmp.text, out _)) continue;
				// Already localized
				if (tmp.gameObject.TryGetComponent<I18NTextBlock>(out _)) continue;
				// Is input field's content
				if (tmp.gameObject.name == "Text" &&
				    tmp.transform.parent.parent.TryGetComponent<TMP_InputField>(out _)) continue;

				texts.Add(new TextInfo
					{ Path = GetObjectPath(tmp.gameObject), Text = tmp.text, Key = string.Empty });
			}

			var dir = Path.GetDirectoryName(JsonPath);
			if (dir is null)
			{
				Debug.LogWarning("Cannot Get the directory to save i18n file");
				return;
			}

			Directory.CreateDirectory(dir);

			string json = JsonConvert.SerializeObject(texts, Formatting.Indented);
			File.WriteAllText(JsonPath, json);
			AssetDatabase.Refresh();

			Debug.Log($"Exported {texts.Count} unique TMP texts to {JsonPath}");
		}

		[MenuItem("Tools/Localization/Apply localization from json")]
		public static void ApplyLocalizationFromJson()
		{
			if (!File.Exists(JsonPath))
			{
				Debug.LogError($"Localization file not found at {JsonPath}");
				return;
			}

			string json = File.ReadAllText(JsonPath);
			List<TextInfo>? serialDict;
			try
			{
				serialDict = JsonConvert.DeserializeObject<List<TextInfo>>(json);
			}
			catch (Exception ex)
			{
				Debug.LogError($"Failed to parse JSON: {ex.Message}");
				return;
			}

			if (serialDict is null)
			{
				Debug.LogError("Invalid JSON format.");
				return;
			}

			int appliedCount = 0;
			foreach (var info in serialDict)
			{
				if (string.IsNullOrEmpty(info.Key)) continue;
				var go = GetObjectFromPath(info.Path);
				if (go is null || !go.TryGetComponent<TextMeshProUGUI>(out var tmp))
				{
					Debug.LogWarning($"Failed to get TextMeshProUGUI component from {info.Path}");
					continue;
				}

				appliedCount++;
				var textBlock = go.AddComponent<I18NTextBlock>();
				textBlock.Text = tmp;
				textBlock.SetText(info.Key);
			}

			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

			Debug.Log($"Applied localization to {appliedCount} TMP components from {JsonPath}");
		}

		public static string GetObjectPath(GameObject go)
		{
			string path = go.name;
			var parent = go.transform.parent;
			while (parent != null)
			{
				path = $"{parent.name}/{path}";
				parent = parent.transform.parent;
			}

			return path;
		}

		public static GameObject? GetObjectFromPath(string path)
		{
			var names = path.Split('/');
			GameObject? go = GameObject.Find(names[0]);
			if (go == null) return null;
			foreach (var name in names[1..])
			{
				go = go.transform.Find(name).gameObject;
				if (go == null) return null;
			}

			return go;
		}
	}
}

#endif