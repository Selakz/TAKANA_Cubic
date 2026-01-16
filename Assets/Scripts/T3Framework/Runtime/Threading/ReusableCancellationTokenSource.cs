#nullable enable

using System;
using System.Threading;

namespace T3Framework.Runtime.Threading
{
	public sealed class ReusableCancellationTokenSource : IDisposable
	{
		private CancellationTokenSource? cts;

		public CancellationToken Token => cts?.Token ?? CancellationToken.None;

		public ReusableCancellationTokenSource(bool newOnConstruct = false)
		{
			if (newOnConstruct) cts = new CancellationTokenSource();
		}

		public void Cancel()
		{
			cts?.Cancel();
			cts?.Dispose();
			cts = null;
		}

		public void CancelAndReset()
		{
			cts?.Cancel();
			cts?.Dispose();
			cts = new();
		}

		public void Dispose() => cts?.Dispose();
	}
}