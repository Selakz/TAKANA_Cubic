#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Modifier;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.EditPanel.Track
{
	public class EditMoveListContent : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private LayoutElement layoutElement = default!;

		[field: SerializeField]
		public Transform ListItemRoot { get; set; } = default!;

		public Modifier<float> HeightModifier => heightModifier ??=
			new(() => layoutElement.minHeight, value => layoutElement.minHeight = value);

		// Private
		private Modifier<float>? heightModifier;

		// Defined Functions
		public void SortListItems(Comparison<PrefabHandler> comparison)
		{
			List<PrefabHandler> items = new(ListItemRoot.childCount);
			for (var i = 0; i < ListItemRoot.childCount; i++)
			{
				if (ListItemRoot.GetChild(i).TryGetComponent<PrefabHandler>(out var handler))
				{
					items.Add(handler);
				}
			}

			items.Sort(comparison);
			for (var i = 0; i < items.Count; i++)
			{
				items[i].transform.SetSiblingIndex(i);
			}
		}
	}
}
