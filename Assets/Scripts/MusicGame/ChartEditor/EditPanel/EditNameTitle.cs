using MusicGame.ChartEditor.EditingComponents;
using T3Framework.Runtime.Extensions;
using TMPro;
using UnityEngine;

namespace MusicGame.ChartEditor.EditPanel
{
	public class EditNameTitle : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private GameObject singleIdTitleRoot;
		[SerializeField] private TMP_Text singleIdText;
		[SerializeField] private GameObject compositeTitleRoot;
		[SerializeField] private TMP_Text compositeIdText;
		[SerializeField] private TMP_InputField nameInputField;

		public EditingComponent Model
		{
			get => model;
			set
			{
				model = value;
				OnNameInputFieldEndEdit(model.Properties.Get("name", string.Empty));
			}
		}

		// Private
		private EditingComponent model;

		// Event Handlers
		private void OnNameInputFieldSelect(string content)
		{
			singleIdTitleRoot.SetActive(false);
			compositeTitleRoot.SetActive(true);
		}

		private void OnNameInputFieldEndEdit(string content)
		{
			if (string.IsNullOrEmpty(content))
			{
				Model.Properties.Remove("name");
				singleIdTitleRoot.SetActive(true);
				compositeTitleRoot.SetActive(false);
			}
			else
			{
				Model.Properties["name"] = content;
				singleIdTitleRoot.SetActive(false);
				compositeTitleRoot.SetActive(true);
			}

			singleIdText.text = model.Id.ToString();
			compositeIdText.text = model.Id.ToString();
			nameInputField.SetTextWithoutNotify(model.Properties.Get("name", string.Empty));
		}

		// System Functions
		void Awake()
		{
			nameInputField.onEndEdit.AddListener(OnNameInputFieldEndEdit);
			nameInputField.onSelect.AddListener(OnNameInputFieldSelect);
		}
	}
}