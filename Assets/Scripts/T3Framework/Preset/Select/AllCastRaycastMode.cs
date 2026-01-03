#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime.ECS;
using UnityEngine;

namespace T3Framework.Preset.Select
{
	public class AllCastRaycastMode<T> : ISelectRaycastMode<T> where T : IComponent
	{
		private readonly bool isCtrl;

		private AllCastRaycastMode(bool isCtrl) => this.isCtrl = isCtrl;

		public static AllCastRaycastMode<T> InstanceSole { get; } =
			new Lazy<AllCastRaycastMode<T>>(() => new(false)).Value;

		public static AllCastRaycastMode<T> InstanceCtrl { get; } =
			new Lazy<AllCastRaycastMode<T>>(() => new(true)).Value;

		public void HandleSelect(ISelectDataset<T> dataset, KeyValuePair<RaycastHit, T>[] hitData, int length = -1)
		{
			length = length == -1 ? hitData.Length : length;
			if (!isCtrl) dataset.Clear();
			for (int i = 0; i < length; i++)
			{
				var pair = hitData[i];
				dataset.ToggleSelecting(pair.Value);
			}
		}
	}
}