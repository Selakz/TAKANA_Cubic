#nullable enable

using System;
using UnityEngine;
using UnityEngine.EventSystems;

// Resharper disable InconsistentNaming
namespace T3Framework.Runtime.Event.UI
{
	public class PointerUpDownListener : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
	{
		public event Action<PointerEventData>? PointerUp;
		public event Action<PointerEventData>? PointerDown;
		public event Action<PointerEventData>? PointerClick;

		public void OnPointerUp(PointerEventData eventData)
		{
			PointerUp?.Invoke(eventData);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			PointerDown?.Invoke(eventData);
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			PointerClick?.Invoke(eventData);
		}
	}
}