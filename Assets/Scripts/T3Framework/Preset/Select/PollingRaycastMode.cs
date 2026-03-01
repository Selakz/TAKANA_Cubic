#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime.ECS;
using UnityEngine;

namespace T3Framework.Preset.Select
{
	public class PollingRaycastMode<T> : ISelectRaycastMode<T> where T : IComponent
	{
		private readonly IComparer<KeyValuePair<RaycastHit, T>> comparer;
		private readonly bool isCtrl;

		public static PollingRaycastMode<T> InstanceSole { get; } =
			new Lazy<PollingRaycastMode<T>>(() => new()).Value;

		public static PollingRaycastMode<T> InstanceCtrl { get; } =
			new Lazy<PollingRaycastMode<T>>(() => new(null, true)).Value;

		public PollingRaycastMode(IComparer<KeyValuePair<RaycastHit, T>>? comparer = null, bool isCtrl = false)
		{
			this.comparer = comparer ?? Comparer<KeyValuePair<RaycastHit, T>>.Create(
				(x, y) => x.Key.colliderInstanceID.CompareTo(y.Key.colliderInstanceID));
			this.isCtrl = isCtrl;
		}

		public void HandleSelect(ISelectDataset<T> dataset, KeyValuePair<RaycastHit, T>[] hitData, int length = -1)
		{
			length = length == -1 ? hitData.Length : length;
			if (length == 0)
			{
				if (!isCtrl) dataset.Clear();
				return;
			}

			if (length == 1 && isCtrl)
			{
				dataset.ToggleSelecting(hitData[0].Value);
				return;
			}

			Array.Sort(hitData, 0, length, comparer);
			T? toSelect = default;
			for (int i = 0; i < length; i++)
			{
				var pair = hitData[i];
				if (dataset.Contains(pair.Value))
				{
					var next = hitData[(i + 1) % length].Value;
					toSelect = dataset.Contains(next) ? pair.Value : next;
				}
			}

			toSelect ??= hitData[0].Value;
			if (!isCtrl) dataset.Clear();
			dataset.Add(toSelect);
		}
	}
}