using MusicGame.ChartEditor.Level;
using MusicGame.ChartEditor.Message;
using MusicGame.ChartEditor.Select;
using T3Framework.Runtime.Extensions;
using TMPro;
using UnityEngine;

namespace MusicGame.ChartEditor.EditPanel
{
	[RequireComponent(typeof(TMP_InputField))]
	public class ComponentSearchInputField : MonoBehaviour
	{
		// Private
		private TMP_InputField searchInputField;

		// Event Handler
		private static void OnSearchInputFieldEndEdit(string content)
		{
			bool isFind = false;
			if (int.TryParse(content, out int targetId))
			{
				if (IEditingChartManager.Instance.Chart.Contains(targetId))
				{
					isFind = true;
					ISelectManager.Instance.UnselectAll();
					ISelectManager.Instance.Select(targetId);
				}
			}
			else if (!string.IsNullOrEmpty(content))
			{
				foreach (var component in IEditingChartManager.Instance.Chart)
				{
					string targetName = component.Properties.Get("name", string.Empty);
					if (!string.IsNullOrEmpty(targetName) && targetName.StartsWith(content))
					{
						if (!isFind)
						{
							isFind = true;
							ISelectManager.Instance.UnselectAll();
						}

						ISelectManager.Instance.Select(component.Id);
					}
				}
			}

			if (!isFind)
			{
				HeaderMessage.Show("没有找到指定的元件", HeaderMessage.MessageType.Info);
			}
		}

		// System Function
		void Awake()
		{
			searchInputField = GetComponent<TMP_InputField>();
			searchInputField.onEndEdit.AddListener(OnSearchInputFieldEndEdit);
		}
	}
}