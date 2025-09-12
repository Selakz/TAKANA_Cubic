#nullable enable

using System.Collections.Generic;

namespace T3Framework.Runtime.Event
{
	public class VetoArg
	{
		private readonly List<string> reasons = new();

		public bool CanExecute { get; private set; } = true;

		public IEnumerable<string> Reasons => reasons;

		public void Veto(string? reason = null)
		{
			CanExecute = false;
			if (reason != null) reasons.Add(reason);
		}
	}
}