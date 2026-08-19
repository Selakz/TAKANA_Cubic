#nullable enable

using MusicGame.ChartEditor.TrackLayer;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared.To
{
	public class RawLayerData
	{
		public int id { get; }

		public string name { get; }

		public RawColorData color { get; }

		public bool isDecoration { get; }

		public bool isSelected { get; }

		public RawLayerData(LayerInfo info)
		{
			id = info.Id;
			name = info.Name;
			color = new RawColorData(info.Color);
			isDecoration = info.IsDecoration;
			isSelected = info.IsSelected;
		}
	}
}
