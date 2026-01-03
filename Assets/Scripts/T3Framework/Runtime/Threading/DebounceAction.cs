#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace T3Framework.Runtime.Threading
{
	public class DebounceAction
	{
		private CancellationTokenSource? cts;

		public DebounceAction()
		{
		}

		public void Invoke(Action action, T3Time delay)
		{
			cts?.Cancel();
			cts?.Dispose();
			cts = new CancellationTokenSource();
			InvokeAsync(action, delay, cts.Token).Forget();
		}

		public void Cancel()
		{
			cts?.Cancel();
			cts?.Dispose();
		}

		private static async UniTaskVoid InvokeAsync(Action action, T3Time delay, CancellationToken token)
		{
			if (!token.IsCancellationRequested)
			{
				await UniTask.Delay(delay.Milli, cancellationToken: token);
				action.Invoke();
			}
		}
	}
}