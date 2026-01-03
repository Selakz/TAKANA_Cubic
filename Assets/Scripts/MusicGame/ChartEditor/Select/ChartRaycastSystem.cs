#nullable enable

using MusicGame.Gameplay.Chart;
using T3Framework.Preset.Select;
using T3Framework.Runtime.ECS;
using UnityEngine;

namespace MusicGame.ChartEditor.Select
{
	public class ChartRaycastSystem : SelectRaycastSystem<ChartComponent>
	{
		public IViewPool<ChartComponent>? ChartViewPool { private get; set; }

		public ChartRaycastSystem(ISelectDataset<ChartComponent> dataset, ISelectRayMaker rayMaker, int bufferSize)
			: base(dataset, rayMaker, bufferSize)
		{
		}

		public override ChartComponent? GetData(Collider collider)
		{
			if (ChartViewPool is null) return null;
			var handler = collider.transform.GetComponent<PrefabHandler>().Parent;
			return handler is null ? null : ChartViewPool[handler];
		}
	}
}