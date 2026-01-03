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
			// Ray ray = RayMaker.GetRay();
			// var hitCount = Physics.RaycastNonAlloc(ray.origin, ray.direction, hitBuffer, MaxDistance, RaycastLayerMask);
			// var dataCount = 0;
			// for (var i = 0; i < hitCount; i++)
			// {
			// 	var hit = hitBuffer[i];
			// 	var data = GetData(hit);
			// 	if (data is null) continue;
			// 	dataBuffer[dataCount] = new(hit, data);
			// 	dataCount++;
			// }
			//
			// raycastMode.HandleSelect(dataset, dataBuffer, dataCount);
			// return dataBuffer.AsSpan(0, dataCount);

			var span = DoRaycastNonSelect();
			raycastMode.HandleSelect(dataset, dataBuffer, span.Length);
			return span;
		}
	}
}