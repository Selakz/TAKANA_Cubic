using System.Threading;
using Cysharp.Threading.Tasks;
using T3Framework.Static.Event;
using UnityEngine;

namespace T3Framework.Preset.Drag
{
	public abstract class DragHelper
	{
		// Serializable and Public
		public NotifiableProperty<bool> IsDragging { get; } = new(false);

		public abstract int DragThreshold { get; }
		public abstract Vector3 CurrentScreenPoint { get; }

		// Private
		private Vector3? beginScreenPosition;
		private CancellationTokenSource cts;

		// Defined Functions
		/// <returns> True: Begin to check / False: Already have an ongoing dragging </returns>
		public bool BeginDrag()
		{
			if (IsDragging.Value || beginScreenPosition.HasValue) return false;

			beginScreenPosition = CurrentScreenPoint;
			cts?.Cancel();
			cts = new CancellationTokenSource();
			CheckAndDragging(cts.Token).Forget();
			return true;
		}

		private async UniTaskVoid CheckAndDragging(CancellationToken token)
		{
			while (beginScreenPosition.HasValue && !IsDragging.Value && !token.IsCancellationRequested)
			{
				var delta = beginScreenPosition.Value - CurrentScreenPoint;
				if (delta.sqrMagnitude > DragThreshold * DragThreshold)
				{
					IsDragging.Value = true;
					beginScreenPosition = null;
					BeginDragLogic();
					break;
				}

				await UniTask.Yield(PlayerLoopTiming.Update, token);
			}

			while (IsDragging.Value && !token.IsCancellationRequested)
			{
				OnDraggingLogic();
				await UniTask.Yield(PlayerLoopTiming.Update, token);
			}
		}

		/// <returns> True: End the ongoing dragging / False: No ongoing dragging to end </returns>
		public bool EndDrag()
		{
			if (!IsDragging.Value)
			{
				CancelDrag();
				return false;
			}

			EndDragLogic();
			CancelDrag();
			return true;
		}

		public void CancelDrag()
		{
			cts?.Cancel();
			cts?.Dispose();
			cts = null;

			CancelDragLogic();
			beginScreenPosition = null;
			IsDragging.Value = false;
		}

		protected abstract void BeginDragLogic();
		protected abstract void OnDraggingLogic();
		protected abstract void EndDragLogic();
		protected abstract void CancelDragLogic();
	}
}