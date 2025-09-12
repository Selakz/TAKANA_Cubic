using System.Collections.Generic;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.Components;
using MusicGame.Components.Chart;
using MusicGame.Components.JudgeLines;

namespace MusicGame.ChartEditor.Level
{
	public interface IEditingChartManager
	{
		public IJudgeLine DefaultJudgeLine { get; }

		public ChartInfo Chart { get; }

		public static IEditingChartManager Instance { get; protected set; }

		/// <summary> Will do nothing and return true if doing duplicate addition;<br/> Will return false if parent do not exist. </summary>
		public bool AddComponent(IComponent component);

		/// <summary> If any of the components cannot be added, all of them won't be added. </summary>
		public bool AddComponents(IEnumerable<IComponent> components);

		/// <summary> Will also recursively remove all children of the specified component. </summary>
		/// <returns> All components that's deleted, sorted by hierarchy order. </returns>
		public IEnumerable<EditingComponent> RemoveComponent(int id);

		/// <summary> Will also recursively remove all children of the specified components. </summary>
		/// <returns> All components that's deleted, sorted by hierarchy order. </returns>
		public IEnumerable<EditingComponent> RemoveComponents(IEnumerable<int> ids);

		/// <summary> Should be called if updating the chart's properties externally. </summary>
		public void UpdateProperties();

		/// <summary> Should be called if updating a component's properties externally. </summary>
		public void UpdateComponent(int id);

		/// <summary> Should be called if updating components' properties externally. </summary>
		public void UpdateComponents(IEnumerable<int> ids);

		/// <summary> Get all direct children of the specified component. </summary>
		public IEnumerable<EditingComponent> GetChildrenComponents(int id);
	}
}