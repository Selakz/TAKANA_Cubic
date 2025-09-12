#nullable enable

using MusicGame.ChartEditor.EditingComponents;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Setting;

namespace MusicGame.ChartEditor.TrackLayer
{
	public static class LayerExtension
	{
		public static void SetLayer(this EditingTrack editingTrack, int layerId)
		{
			editingTrack.Properties["layer"] = layerId;
		}

		public static LayerInfo GetLayer(this EditingTrack editingTrack)
		{
			var id = editingTrack.Properties.Get("layer", ISingletonSetting<TrackLayerSetting>.Instance.DefaultLayerId);
			TrackLayerManager.Instance.TryGetLayer(id, out var layerInfo);
			return layerInfo;
		}
	}
}