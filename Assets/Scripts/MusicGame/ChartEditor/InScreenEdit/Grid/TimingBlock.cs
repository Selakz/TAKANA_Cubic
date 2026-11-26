#nullable enable

using System;
using T3Framework.Runtime;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	public class TimingBlock : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
		// Serializable and Public
		[SerializeField] private Image image = default!;

		public NotifiableProperty<bool> IsHovered { get; } = new(false);

		public event Action<T3Time>? OnClicked;

		// Private
		private T3Time time;

		// Defined Functions
		public void Init(T3Time time, Color color)
		{
			this.time = time;
			image.color = color;
		}

		// System Functions
		public void OnPointerClick(PointerEventData eventData)
		{
			GUIUtility.systemCopyBuffer = time.ToString();
			if (TimingInputField.Current != null)
			{
				TimingInputField.Current.text = time.ToString();
				TimingInputField.Current.onEndEdit.Invoke(time.ToString());
			}
			else
			{
				OnClicked?.Invoke(time);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			transform.SetAsLastSibling();
			transform.localScale = new(1, 1.5f);
			IsHovered.Value = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			transform.localScale = new(1, 1);
			IsHovered.Value = false;
		}

		void OnDisable()
		{
			transform.localScale = new(1, 1);
			IsHovered.Value = false;
		}
	}
}