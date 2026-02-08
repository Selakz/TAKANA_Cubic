#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using T3Framework.Runtime.Threading;

namespace T3Framework.Runtime.Modifier
{
	public struct ModifierItem<T, TMethod>
	{
		public TMethod Method { get; set; }

		public T Value { get; set; }
	}

	public abstract class BaseModifier<T, TMethod> : IModifier<T>
	{
		private readonly Func<T> getter;
		private readonly Action<T> setter;
		private readonly Func<T, T> resetFunction;
		private readonly SortedList<int, ModifierItem<T, TMethod>> steps = new();

		private readonly ReusableCancellationTokenSource rcts = new();
		private bool isModified = false;

		/// <summary>
		/// The timing to delay the modify process to avoid modify multiple times in one frame.
		/// Specially, <see cref="PlayerLoopTiming.Initialization"/> means modifying immediately and
		/// <see cref="PlayerLoopTiming.LastInitialization"/> means do not modify until <see cref="Update"/> is called.
		/// </summary>
		public PlayerLoopTiming DefaultUpdatingTime { get; set; } = PlayerLoopTiming.Initialization;

		public T Value => getter.Invoke();

		protected abstract T ModifyStep(TMethod method, T lastValue, T thisValue);

		private void DoModify()
		{
			var value = Value;
			value = resetFunction.Invoke(value);
			value = steps.Values.Aggregate(value, (current, stepsValue) =>
				ModifyStep(stepsValue.Method, current, stepsValue.Value));
			setter.Invoke(value);
			isModified = false;
		}

		protected BaseModifier(Func<T> getter, Action<T> setter, Func<T, T> resetFunction)
		{
			this.getter = getter;
			this.setter = setter;
			this.resetFunction = resetFunction;
			DoModify();
		}

		protected BaseModifier(Func<T> getter, Action<T> setter, T defaultValue) : this(getter, setter,
			_ => defaultValue)
		{
		}

		protected BaseModifier(Func<T> getter, Action<T> setter)
		{
			var defaultValue = getter.Invoke();
			this.getter = getter;
			this.setter = setter;
			resetFunction = _ => defaultValue;
			DoModify();
		}

		public void Register(TMethod method, T value, int priority, PlayerLoopTiming? updateTime = null)
		{
			updateTime ??= DefaultUpdatingTime;
			steps[priority] = new ModifierItem<T, TMethod> { Method = method, Value = value };
			isModified = true;
			DecideDoModify(updateTime.Value);
		}

		public abstract void Assign(T value, int priority);

		public void Unregister(int priority, PlayerLoopTiming? updateTime)
		{
			updateTime ??= DefaultUpdatingTime;
			steps.Remove(priority);
			isModified = true;
			DecideDoModify(updateTime.Value);
		}

		public void Unregister(int priority) => Unregister(priority, null);

		private void DecideDoModify(PlayerLoopTiming updateTime)
		{
			switch (updateTime)
			{
				case PlayerLoopTiming.Initialization:
					DoModify();
					break;
				case PlayerLoopTiming.LastInitialization:
					break;
				default:
					rcts.CancelAndReset();
					UniTask.Yield(updateTime, cancellationToken: rcts.Token).ContinueWith(DoModify);
					break;
			}
		}

		public void Update()
		{
			if (!isModified) return;
			DoModify();
		}

		public void Clear() => steps.Clear();
	}
}