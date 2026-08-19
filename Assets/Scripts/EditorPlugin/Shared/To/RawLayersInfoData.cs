#nullable enable

using System.Linq;
using MusicGame.ChartEditor.TrackLayer;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared.To
{
	public class RawLayersInfoData
	{
		private readonly LayersInfo layersInfo;

		public RawLayerData[] layers => layersInfo.Select(layer => new RawLayerData(layer.Model)).ToArray();

		public RawLayerData defaultLayer => new(layersInfo.DefaultLayer);

		public RawLayersInfoData(LayersInfo layersInfo)
		{
			this.layersInfo = layersInfo;
		}

		public bool add(LayerInfo info)
		{
			info.Id = layersInfo.NewId;
			return layersInfo.Add(new LayerComponent(layersInfo, info));
		}

		public bool remove(int layerId)
		{
			if (layersInfo.FirstOrDefault(l => l.Model.Id == layerId) is not { } layer) return false;
			return layersInfo.Remove(layer);
		}

		public bool update(int layerId, LayerInfo info)
		{
			if (layersInfo.FirstOrDefault(l => l.Model.Id == layerId) is not { } layer) return false;
			layer.UpdateModel(model =>
			{
				model.Name = info.Name;
				model.Color = info.Color;
				model.IsDecoration = info.IsDecoration;
				model.IsSelected = info.IsSelected;
			});
			return true;
		}
	}
}