#nullable enable

using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Timer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace T3Framework.Preset.UICollection
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

	public readonly struct DoubleClickButtonRegistrar : IEventRegistrar
	{
		public enum RegisterTarget
		{
			First,
			Second
		}

		private readonly DoubleClickButton button;
		private readonly RegisterTarget registerTarget;
		private readonly UnityAction action;

		public DoubleClickButtonRegistrar(DoubleClickButton button, RegisterTarget registerTarget, UnityAction action)
		{
			this.button = button;
			this.registerTarget = registerTarget;
			this.action = action;
		}

		public void Register()
		{
			switch (registerTarget)
			{
				case RegisterTarget.First:
					button.OnFirstClick += action;
					break;
				case RegisterTarget.Second:
					button.OnSecondClick += action;
					break;
			}
		}

		public void Unregister()
		{
			switch (registerTarget)
			{
				case RegisterTarget.First:
					button.OnFirstClick -= action;
					break;
				case RegisterTarget.Second:
					button.OnSecondClick -= action;
					break;
			}
		}
	}
}