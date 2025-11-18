#nullable enable

using System;
using UnityEngine;
using UnityEngine.EventSystems;

// ReSharper disable InconsistentNaming
namespace T3Framework.Runtime.Event.UI
{
	public class PointerEnterExitListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		// Serializable and Public
		public event Action<PointerEventData>? PointerEnter;
		public event Action<PointerEventData>? PointerExit;

		public void OnPointerEnter(PointerEventData eventData)
		{
			PointerEnter?.Invoke(eventData);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			PointerExit?.Invoke(eventData);
		}

		// Private
		private static readonly PointerEventData defaultData = new(EventSystem.current);

		// System Functions
		void OnDisable()
		{
			PointerExit?.Invoke(defaultData);
		}
	}
}