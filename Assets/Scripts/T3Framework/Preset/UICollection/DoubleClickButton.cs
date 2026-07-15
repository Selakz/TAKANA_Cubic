#nullable enable

using Cysharp.Threading.Tasks;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace T3Framework.Preset.UICollection
{
	[RequireComponent(typeof(Button))]
	public class DoubleClickButton : MonoBehaviour
	{
		// Serializable and Public
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
		public event UnityAction? OnFirstClickCancelled;
		public event UnityAction? OnSecondClick;

		// Private
		private Button button = default!;
		private bool hasFirstClicked = false;
		private readonly ReusableCancellationTokenSource rcts = new();

		// Event Handlers
		private void OnButtonClicked()
		{
			rcts.CancelAndReset();
			if (!hasFirstClicked)
			{
				hasFirstClicked = true;
				UniTask.Delay(ClickInterval.Milli, cancellationToken: rcts.Token).ContinueWith(
					() =>
					{
						hasFirstClicked = false;
						OnFirstClickCancelled?.Invoke();
					});
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
		}
	}

	public readonly struct DoubleClickButtonRegistrar : IEventRegistrar
	{
		public enum RegisterTarget
		{
			First,
			FirstCancelled,
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
				case RegisterTarget.FirstCancelled:
					button.OnFirstClickCancelled += action;
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
				case RegisterTarget.FirstCancelled:
					button.OnFirstClickCancelled -= action;
					break;
				case RegisterTarget.Second:
					button.OnSecondClick -= action;
					break;
			}
		}
	}
}