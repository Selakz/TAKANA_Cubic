#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime.ECS;
using UnityEngine;

namespace T3Framework.Preset.Select
{
	public abstract class SelectRaycastSystem<T> where T : IComponent
	{
		// Serializable and Public
		public ISelectRayMaker RayMaker { get; set; }

		public LayerMask RaycastLayerMask { get; set; }

		public float MaxDistance { get; set; } = float.MaxValue;

		// Private
		private readonly ISelectDataset<T> dataset;
		private readonly RaycastHit[] hitBuffer;
		private readonly KeyValuePair<RaycastHit, T>[] dataBuffer;

		// Static

		// Defined Functions
		protected SelectRaycastSystem(ISelectDataset<T> dataset, ISelectRayMaker rayMaker, int bufferSize)
		{
			this.dataset = dataset;
			RayMaker = rayMaker;
			hitBuffer = new RaycastHit[bufferSize];
			dataBuffer = new KeyValuePair<RaycastHit, T>[bufferSize];
		}

		public abstract T? GetData(Collider collider);

		public ReadOnlySpan<KeyValuePair<RaycastHit, T>> DoRaycastNonSelect()
		{
			Ray ray = RayMaker.GetRay();
			var hitCount = Physics.RaycastNonAlloc(ray.origin, ray.direction, hitBuffer, MaxDistance, RaycastLayerMask);
			var dataCount = 0;
			for (var i = 0; i < hitCount; i++)
			{
				var hit = hitBuffer[i];
				var data = GetData(hit.collider);
				if (data is null) continue;
				dataBuffer[dataCount] = new(hit, data);
				dataCount++;
			}

			return dataBuffer.AsSpan(0, dataCount);
		}

		public ReadOnlySpan<KeyValuePair<RaycastHit, T>> DoRaycast(ISelectRaycastMode<T> raycastMode)
		{
			var span = DoRaycastNonSelect();
			raycastMode.HandleSelect(dataset, dataBuffer, span.Length);
			return span;
		}
	}

	public class ViewSelectRaycastSystem<T> : SelectRaycastSystem<T> where T : IComponent
	{
		public IViewPool<T>? ViewPool { private get; set; }

		private readonly Func<Collider, PrefabHandler> getHandlerFunc;

		public ViewSelectRaycastSystem(
			ISelectDataset<T> dataset, ISelectRayMaker rayMaker, int bufferSize,
			Func<Collider, PrefabHandler> getHandlerFunc) :
			base(dataset, rayMaker, bufferSize)
		{
			this.getHandlerFunc = getHandlerFunc;
		}

		public override T? GetData(Collider collider)
		{
			if (ViewPool is null) return default;
			var handler = getHandlerFunc.Invoke(collider);
			return handler is null ? default : ViewPool[handler];
		}
	}
}