#nullable enable

using MusicGame.ChartEditor.TrackLayer;
using MusicGame.Gameplay.Chart;

namespace EditorPlugin.Shared
{
	/// <summary>
	/// What a Raw*Data may need to know about the world around the component it wraps: its parent, the snapshot the
	/// plugin sees of another component, and the layer a track belongs to.
	/// Chart components answer through the chart they are in; clipboard copies answer through the ClipboardItem they
	/// were copied from, because they belong to no chart.
	/// </summary>
	public interface IComponentRelations
	{
		/// <summary> The parent component, or null when the component has none. </summary>
		public ChartComponent? GetParent(ChartComponent component);

		/// <summary> The raw snapshot the plugin sees of the component, or null when the plugin cannot see it. </summary>
		public object? GetSnapshot(ChartComponent component);

		/// <summary> The layer the track belongs to, or null when it cannot be resolved. </summary>
		public LayerInfo? GetLayer(ChartComponent track);
	}
}
