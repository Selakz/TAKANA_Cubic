#nullable enable

using System;
using T3Framework.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.InScreenEdit.UI
{
	public class EditBpmContent : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField timingInputField = default!;
		[SerializeField] private TMP_InputField bpmInputField = default!;
		[SerializeField] private Button deleteButton = default!;
		[SerializeField] private Button addButton = default!;

		public event Action<T3Time, T3Time, float>? OnBpmChanged;

		// Private
		private T3Time time = 0;
		private float bpm = 100f;

		// Static

		// Defined Functions
		public void Init(T3Time time, float bpm)
		{
			this.time = time;
			this.bpm = bpm;
			timingInputField.text = time.ToString();
			bpmInputField.text = bpm.ToString("0.000");
			transform.localScale = Vector3.one;
		}

		// Event Handlers
		private void OnTimingInputFieldEndEdit(string content)
		{
			if (int.TryParse(content, out int newTime) && newTime >= 0 && newTime != time)
			{
				OnBpmChanged?.Invoke(time, newTime, bpm);
				time = newTime;
				return;
			}

			timingInputField.text = time.ToString();
		}

		private void OnBpmInputFieldEndEdit(string content)
		{
			if (float.TryParse(content, out float newBpm) && newBpm > 0)
			{
				bpm = newBpm;
				OnBpmChanged?.Invoke(time, time, bpm);
				return;
			}

			bpmInputField.text = bpm.ToString("0.000");
		}

		private void OnDeleteButtonPressed()
		{
			OnBpmChanged?.Invoke(time, T3Time.MinValue, bpm);
		}

		private void OnAddButtonPressed()
		{
			OnBpmChanged?.Invoke(T3Time.MinValue, time + 1000, bpm);
		}

		// System Functions
		void Awake()
		{
			timingInputField.onEndEdit.AddListener(OnTimingInputFieldEndEdit);
			bpmInputField.onEndEdit.AddListener(OnBpmInputFieldEndEdit);
			deleteButton.onClick.AddListener(OnDeleteButtonPressed);
			addButton.onClick.AddListener(OnAddButtonPressed);
		}
	}
}