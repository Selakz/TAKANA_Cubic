#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.ListRender;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.UI
{
	[RequireComponent(typeof(ListRendererInt))]
	public class BpmListManager : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private GridTimeRetriever gridTimeRetriever = default!;

		public ListRendererInt ListRenderer { get; private set; } = default!;

		// Private

		// Event Handlers
		private void OnBpmChanged(T3Time oldTime, T3Time newTime, float bpm)
		{
			if (oldTime != T3Time.MinValue)
			{
				gridTimeRetriever.BpmList.Remove(oldTime);
			}

			if (newTime != T3Time.MinValue)
			{
				gridTimeRetriever.BpmList.Add(newTime, bpm);
			}
		}

		private void EditOnBpmListUpdate()
		{
			foreach (var go in ListRenderer.Values)
			{
				var content = go.GetComponent<EditBpmContent>();
				content.OnBpmChanged -= OnBpmChanged;
			}

			ListRenderer.Clear();
			foreach (var pair in gridTimeRetriever.BpmList)
			{
				var content = ListRenderer.Add<EditBpmContent>(pair.Key);
				content.Init(pair.Key, pair.Value);
				content.OnBpmChanged += OnBpmChanged;
			}
		}

		// System Functions
		void Awake()
		{
			Dictionary<Type, LazyPrefab> listPrefabs = new()
			{
				[typeof(EditBpmContent)] =
					new LazyPrefab("Prefabs/EditorUI/InScreenEdit/EditBpmContent", "EditBpmContentPrefab_OnLoad")
			};
			ListRenderer = GetComponent<ListRendererInt>();
			ListRenderer.Init(listPrefabs);
			ListRenderer.ListSorter = (a, b) => a.CompareTo(b);
		}

		void OnEnable()
		{
			EventManager.Instance.AddListener("Edit_OnBpmListUpdate", EditOnBpmListUpdate);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener("Edit_OnBpmListUpdate", EditOnBpmListUpdate);
		}
	}
}