using System;
using MusicGame.ChartEditor.Message;
using MusicGame.ChartEditor.TrackLine;
using MusicGame.Components.Movement;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Easing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.EditPanel
{
	public class EditV1EItemContent : MonoBehaviour, IEditMoveItemContent
	{
		// Serializable and Public
		[SerializeField] private Image background;
		[SerializeField] private TMP_InputField timeInputField;
		[SerializeField] private TMP_InputField positionInputField;
		[SerializeField] private TMP_InputField easeInputField;
		[SerializeField] private Button addButton;
		[SerializeField] private Button removeButton;

		public event Action<IMoveItem, IMoveItem> OnMoveItemUpdated;

		public IMoveItem MoveItem
		{
			get => moveItem;
			set
			{
				if (value is not V1EMoveItem v1e)
				{
					Debug.LogError($"{nameof(MoveItem)} is not of type {nameof(V1EMoveItem)}");
					return;
				}

				moveItem = v1e;
				timeInputField.SetTextWithoutNotify(v1e.Time.ToString());
				positionInputField.SetTextWithoutNotify(v1e.Position.ToString("0.000"));
				easeInputField.SetTextWithoutNotify(v1e.Ease.GetString());
				transform.localScale = Vector3.one;
			}
		}

		public Color BackgroundColor
		{
			get => background.color;
			set => background.color = value;
		}

		// Private
		private V1EMoveItem moveItem;

		// Static

		// Defined Functions

		// Event Handlers
		private void OnTimeInputFieldEndEdit(string content)
		{
			V1EMoveItem v1e = (V1EMoveItem)MoveItem;
			if (int.TryParse(content, out int newTime) && newTime != MoveItem.Time)
			{
				OnMoveItemUpdated?.Invoke(MoveItem, new V1EMoveItem(newTime, v1e.Position, v1e.Ease));
			}
			else
			{
				timeInputField.SetTextWithoutNotify(MoveItem.Time.ToString());
			}
		}

		private void OnPositionInputFieldEndEdit(string content)
		{
			V1EMoveItem v1e = (V1EMoveItem)MoveItem;
			if (float.TryParse(content, out float newPosition))
			{
				if (Mathf.Approximately(newPosition, moveItem.Position)) return;
				OnMoveItemUpdated?.Invoke(MoveItem, new V1EMoveItem(v1e.Time, newPosition, v1e.Ease));
			}
			else
			{
				HeaderMessage.Show("ÐÞ¸ÄÊ§°Ü", HeaderMessage.MessageType.Warn);
				positionInputField.SetTextWithoutNotify(moveItem.Position.ToString("0.000"));
			}
		}

		private void OnEaseInputFieldEndEdit(string content)
		{
			V1EMoveItem v1e = (V1EMoveItem)MoveItem;
			Eases newEase = int.TryParse(content, out var curveInt)
				? curveInt >= 100
					? CurveCalculator.GetEaseByRpeNumber(curveInt - 100)
					: CurveCalculator.GetEaseById(curveInt)
				: CurveCalculator.GetEaseByName(content);
			easeInputField.SetTextWithoutNotify(newEase.GetString());
			OnMoveItemUpdated?.Invoke(MoveItem, new V1EMoveItem(v1e.Time, v1e.Position, newEase));
		}

		private void OnAddButtonClick()
		{
			V1EMoveItem v1e = (V1EMoveItem)MoveItem;
			var newTime = MoveItem.Time + ISingletonSetting<TrackLineSetting>.Instance.AddNodeTimeDistance;
			var newItem = new V1EMoveItem(newTime, v1e.Position, Eases.Unmove);
			OnMoveItemUpdated?.Invoke(null, newItem);
		}

		private void OnRemoveButtonClick()
		{
			OnMoveItemUpdated?.Invoke(MoveItem, null);
		}

		// System Functions
		void Awake()
		{
			timeInputField.onEndEdit.AddListener(OnTimeInputFieldEndEdit);
			positionInputField.onEndEdit.AddListener(OnPositionInputFieldEndEdit);
			easeInputField.onEndEdit.AddListener(OnEaseInputFieldEndEdit);
			addButton.onClick.AddListener(OnAddButtonClick);
			removeButton.onClick.AddListener(OnRemoveButtonClick);
		}
	}
}