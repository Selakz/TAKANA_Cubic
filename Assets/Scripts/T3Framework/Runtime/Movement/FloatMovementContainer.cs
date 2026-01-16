#nullable enable

using System;
using DG.Tweening;
using UnityEngine;

namespace T3Framework.Runtime.Movement
{
	// TODO: Allow to add interface to list
	public class FloatMovementContainer : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private int loopCount = 0;
		[SerializeField] private LoopType loopType;
		[SerializeField] private float initialValue;
		[SerializeField] private EaseFloatMoveItem[] moveItems = default!;

		// Private
		private Sequence? sequence;

		// Defined Functions
		public void Move(Func<float> getter, Action<float> setter)
		{
			if (sequence is null)
			{
				sequence = DOTween.Sequence().SetAutoKill(false).Pause();
				if (loopCount is -1 or > 0) sequence.SetLoops(loopCount, loopType);
				foreach (var moveItem in moveItems)
				{
					sequence.Append(DOTween
						.To(getter.Invoke, setter.Invoke, moveItem.Position, moveItem.Duration)
						.SetEase(moveItem.Ease));
				}
			}

			setter.Invoke(initialValue);
			sequence.Goto(0, true);
		}
	}
}