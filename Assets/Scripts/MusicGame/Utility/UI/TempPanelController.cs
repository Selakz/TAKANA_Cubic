#nullable enable

using DG.Tweening;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Event.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MusicGame.ChartEditor.Record.UI
{
	public class TempPanelController : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private PointerEnterExitListener pointerEnterExitListener = default!;
		[SerializeField] private NotifiableDataContainer<bool> isInRecordModeContainer = default!;
		[SerializeField] private GameObject targetPanel = default!;
		[SerializeField] private Vector2 showPosition;
		[SerializeField] private int duration = 1000;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new CustomRegistrar(
				() => pointerEnterExitListener.PointerEnter += Show,
				() => pointerEnterExitListener.PointerEnter -= Show),
			new CustomRegistrar(
				() => pointerEnterExitListener.PointerExit += Hide,
				() => pointerEnterExitListener.PointerEnter -= Hide)
		};

		// Private
		private Vector2 startPosition;
		private Tween? currentTween;

		// Event Handlers
		private void Show(PointerEventData data)
		{
			if (!isInRecordModeContainer.Property.Value) return;
			currentTween?.Kill(true);
			((RectTransform)targetPanel.transform).anchoredPosition = startPosition;
			currentTween = DOTween.To(
				() => ((RectTransform)targetPanel.transform).anchoredPosition,
				x => ((RectTransform)targetPanel.transform).anchoredPosition = x,
				showPosition, ((T3Time)duration).Second);
		}

		private void Hide(PointerEventData data)
		{
			// if (!isInRecordModeContainer.Property.Value) return;
			currentTween?.Kill(true);
			((RectTransform)targetPanel.transform).anchoredPosition = showPosition;
			currentTween = DOTween.To(
				() => ((RectTransform)targetPanel.transform).anchoredPosition,
				x => ((RectTransform)targetPanel.transform).anchoredPosition = x,
				startPosition, ((T3Time)duration).Second);
		}

		// System Functions
		protected override void Awake()
		{
			startPosition = ((RectTransform)targetPanel.transform).anchoredPosition;
		}
	}
}