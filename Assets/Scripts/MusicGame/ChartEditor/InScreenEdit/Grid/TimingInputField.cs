#nullable enable

using System.Collections;
using TMPro;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	[RequireComponent(typeof(TMP_InputField))]
	public class TimingInputField : MonoBehaviour
	{
		// Serializable and Public

		// Private
		private TMP_InputField inputField = default!;

		// Static
		public static TMP_InputField? Current { get; private set; } = null;

		// Defined Functions
		private IEnumerator StoreInputField()
		{
			Current = inputField;
			yield return new WaitForSeconds(0.1f);
			Current = null;
		}

		private void BeginStore(string useless)
		{
			StartCoroutine(StoreInputField());
		}

		// System Functions
		void Awake()
		{
			inputField = GetComponent<TMP_InputField>();
			inputField.onDeselect.AddListener(BeginStore);
		}
	}
}