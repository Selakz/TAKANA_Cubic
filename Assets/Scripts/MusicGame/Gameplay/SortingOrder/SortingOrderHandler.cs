#nullable enable

using MusicGame.Components;
using UnityEngine;

namespace MusicGame.Gameplay.SortingOrder
{
	public class SortingOrderHandler : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private int sortingOrder;

		public int SortingOrder
		{
			get => sortingOrder;
			set => sortingOrder = value;
		}

		// Private
		private IModifiableView2D view = default!;

		// Static
		private const int SortingOrderHandlerPriority = 0;

		// System Functions
		void Awake()
		{
			view = GetComponent<IModifiableView2D>();
			if (view is null)
			{
				Debug.LogError($"{nameof(SortingOrderHandler)} cannot find {nameof(IModifiableView2D)}");
			}
		}

		void OnEnable()
		{
			view.SortingOrderModifier.Register(
				_ => SortingOrder,
				SortingOrderHandlerPriority);
		}
	}
}