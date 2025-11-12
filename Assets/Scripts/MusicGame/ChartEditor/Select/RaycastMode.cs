using System;
using System.Collections.Generic;
using T3Framework.Runtime.MVC;
using UnityEngine;

namespace MusicGame.ChartEditor.Select
{
	public enum RaycastMode
	{
		/// <summary> Select one cast component once a time. </summary>
		OneByOne,

		/// <summary> Select all cast components. </summary>
		AllCasted
	}

	public static class SelectModeExtension
	{
		private const int MaxHitCount = 100;
		private static readonly RaycastHit[] hitBuffer = new RaycastHit[MaxHitCount];
		private static readonly IModel[] modelBuffer = new IModel[MaxHitCount];

		private static readonly IComparer<IModel> idComparer = Comparer<IModel>.Create((a, b) => a.Id.CompareTo(b.Id));

		public static IEnumerable<IModel> DoRaycast(this RaycastMode raycastMode,
			Ray ray, int layerMask, float distance = float.MaxValue)
		{
			var size = Physics.RaycastNonAlloc(ray.origin, ray.direction, hitBuffer, distance, layerMask);
			switch (raycastMode)
			{
				case RaycastMode.OneByOne:
					int count = 0;
					for (int i = 0; i < size; i++)
					{
						if (hitBuffer[i].transform.TryGetComponent<IModelSelectable>(out var modelSelectable))
						{
							modelBuffer[count++] = modelSelectable.Model;
						}
					}

					Array.Sort(modelBuffer, 0, count, idComparer);
					for (int i = 0; i < count; i++)
					{
						if (ISelectManager.Instance.IsSelected(modelBuffer[i].Id))
						{
							if (ISelectManager.Instance.IsSelected(modelBuffer[(i + 1) % count].Id))
							{
								yield return modelBuffer[i];
							}
							else
							{
								yield return modelBuffer[(i + 1) % count];
							}

							yield break;
						}
					}

					if (count > 0) yield return modelBuffer[0];
					yield break;
				case RaycastMode.AllCasted:
					for (int i = 0; i < size; i++)
					{
						if (hitBuffer[i].transform.TryGetComponent<IModelSelectable>(out var modelSelectable))
						{
							yield return modelSelectable.Model;
						}
					}

					yield break;
			}
		}
	}
}