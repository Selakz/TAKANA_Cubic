using System.Collections.Generic;
using MusicGame.Components;

namespace MusicGame.ChartEditor.Select
{
	/// <summary>
	/// Be used to select components in a chart.
	/// </summary>
	public interface ISelectManager
	{
		public static ISelectManager Instance { get; protected set; }

		public IDictionary<int, IComponent> SelectedTargets { get; }

		public IComponent CurrentSelecting { get; }

		public bool Select(int id);

		public void SelectBundle(IEnumerable<int> bundleIds);

		public bool IsSelected(int id);

		public bool Unselect(int id);

		public bool UnselectBundle(IEnumerable<int> bundleIds);

		public void UnselectAll();

		public void ToggleBundle(IEnumerable<int> bundleIds);
	}
}