#nullable enable

using T3Framework.Runtime.Timer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace T3Framework.Runtime.UI
{
	[RequireComponent(typeof(Button))]
	public class DoubleClickButton : MonoBehaviour
	{
		[SerializeField] private int clickInterval = 3000;

		public Button Button
		{
			get
			{
				// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
				button ??= GetComponent<Button>();
				return button;
			}
		}

		public T3Time ClickInterval
		{
			get => clickInterval;
			set => clickInterval = value;
		}

		public event UnityAction? OnFirstClick;
		public event UnityAction? OnSecondClick;

		private Button button = default!;
		private TriggerTimer timer = default!;
		private bool hasFirstClicked = false;

		// Event Handlers
		private void OnTimerTrigger()
		{
			hasFirstClicked = false;
		}

		private void OnButtonClicked()
		{
			timer.Stop();
			if (!hasFirstClicked)
			{
				hasFirstClicked = true;
				timer.TimeDelta = ClickInterval;
				timer.Start();
				OnFirstClick?.Invoke();
			}
			else
			{
				hasFirstClicked = false;
				OnSecondClick?.Invoke();
			}
		}

		// System Functions
		void Awake()
		{
			// ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
			button ??= GetComponent<Button>();
			Button.onClick.AddListener(OnButtonClicked);
			timer = new TriggerTimer(ClickInterval.Milli);
			timer.OnTrigger += OnTimerTrigger;
		}
	}
}