#nullable enable

using T3Framework.Runtime.ECS;
using T3Framework.Static.Event;

namespace T3Framework.Preset.Select
{
	/// <summary> As long as the component is in it, it's selected. </summary>
	public interface ISelectDataset<T> : IDataset<T> where T : IComponent
	{
		public NotifiableProperty<T?> CurrentSelecting { get; }

		public void ToggleSelecting(T component);
	}
}