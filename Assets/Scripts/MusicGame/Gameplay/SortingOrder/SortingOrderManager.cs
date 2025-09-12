#nullable enable

using System.Collections.Generic;
using T3Framework.Runtime.Event;
using UnityEngine;

namespace MusicGame.Gameplay.SortingOrder
{
	public class SortingOrderManager : MonoBehaviour
	{
		// Serializable and Public
		// TODO: A way to elegantly store these data (TextAsset or InspectableDictionary)
		public const int TrackSortingOrder = 0;
		public const int SelectedTrackSortingOrder = 50;
		public const int TrackEdgeSortingOrder = 180;
		public const int TapSortingOrder = 300;
		public const int SelectedTapSortingOrder = 350;
		public const int SlideSortingOrder = 400;
		public const int SelectedSlideSortingOrder = 450;
		public const int HoldSortingOrder = 200;
		public const int SelectedHoldSortingOrder = 250;

		public const int NodeSortingOrder = 550;

		// Private
		private readonly Dictionary<string, int> prefabSortingOrders = new()
		{
			["TrackPrefab_OnLoad"] = TrackSortingOrder,
			["TapPrefab_OnLoad"] = TapSortingOrder,
			["SlidePrefab_OnLoad"] = SlideSortingOrder,
			["HoldPrefab_OnLoad"] = HoldSortingOrder,
		};

		// System Functions
		private void OnEnable()
		{
			foreach (var pair in prefabSortingOrders)
			{
				EventManager.Instance.AddListener<GameObject>(pair.Key, go =>
				{
					var handler = go.AddComponent<SortingOrderHandler>();
					handler.SortingOrder = pair.Value;
				});
			}
		}
	}
}