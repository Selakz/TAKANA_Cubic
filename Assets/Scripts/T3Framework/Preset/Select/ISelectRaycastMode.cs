#nullable enable

using System.Collections.Generic;
using T3Framework.Runtime.ECS;
using UnityEngine;

namespace T3Framework.Preset.Select
{
	public interface ISelectRaycastMode<T> where T : IComponent
	{
		/// <returns> The count of data that are hit </returns>
		public void HandleSelect(ISelectDataset<T> dataset, KeyValuePair<RaycastHit, T>[] hitData, int length = -1);
	}
}