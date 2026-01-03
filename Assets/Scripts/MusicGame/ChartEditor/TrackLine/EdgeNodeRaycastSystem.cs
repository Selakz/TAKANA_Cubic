#nullable enable

using T3Framework.Preset.Select;
using T3Framework.Runtime.ECS;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine
{
	public class EdgeNodeRaycastSystem : SelectRaycastSystem<EdgeNodeComponent>
	{
		public IViewPool<EdgeNodeComponent>? NodeViewPool { private get; set; }

		public EdgeNodeRaycastSystem(ISelectDataset<EdgeNodeComponent> dataset, ISelectRayMaker rayMaker,
			int bufferSize) : base(dataset, rayMaker, bufferSize)
		{
		}

		public override EdgeNodeComponent? GetData(Collider collider)
		{
			if (NodeViewPool is null) return null;
			var handler = collider.transform.parent.GetComponent<PrefabHandler>();
			return handler is null ? null : NodeViewPool[handler];
		}
	}
}