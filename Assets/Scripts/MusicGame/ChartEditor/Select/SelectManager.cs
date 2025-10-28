#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Select.Selectables;
using MusicGame.Components;
using MusicGame.Components.Chart;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using UnityEngine;

namespace MusicGame.ChartEditor.Select
{
	public class SelectManager : MonoBehaviour, ISelectManager
	{
		// Serializable and Public
		public IDictionary<int, IComponent> SelectedTargets => selectedTargets;

		public IComponent? CurrentSelecting { get; private set; }

		// Private
		private readonly Dictionary<int, IComponent> selectedTargets = new();
		private ChartInfo chart = default!;

		// Static
		private static readonly Dictionary<string, Action<GameObject>> registerSelectable = new()
		{
			["TapPrefab_OnLoad"] = go => go.AddComponent<TapSelectable>(),
			["SlidePrefab_OnLoad"] = go => go.AddComponent<SlideSelectable>(),
			["HoldPrefab_OnLoad"] = go => go.AddComponent<HoldSelectable>(),
			["TrackPrefab_OnLoad"] = go => go.AddComponent<TrackSelectable>()
		};

		// Defined Functions
		public bool Select(int id)
		{
			if (selectedTargets.ContainsKey(id) || !chart.Contains(id)) return false;
			var selecting = chart[id]!;
			SelectedTargets.Add(id, selecting);
			CurrentSelecting = selecting;
			EventManager.Instance.Invoke("Selection_OnUpdate");
			return true;
		}

		public void SelectBundle(IEnumerable<int> bundleIds)
		{
			bool isSelecting = false;
			foreach (var id in bundleIds)
			{
				if (selectedTargets.ContainsKey(id) || !chart.Contains(id)) continue;
				isSelecting = true;
				var selecting = chart[id]!;
				SelectedTargets.Add(id, selecting);
				CurrentSelecting = selecting;
			}

			if (isSelecting) EventManager.Instance.Invoke("Selection_OnUpdate");
		}

		public bool IsSelected(int id) => selectedTargets.ContainsKey(id);

		public bool Unselect(int id)
		{
			if (!selectedTargets.ContainsKey(id) || !chart.Contains(id)) return false;
			var unselecting = selectedTargets[id];
			SelectedTargets.Remove(id);
			if (CurrentSelecting == unselecting)
			{
				CurrentSelecting = selectedTargets.FirstOrDefault().Value;
			}

			EventManager.Instance.Invoke("Selection_OnUpdate");
			return true;
		}

		public bool UnselectBundle(IEnumerable<int> bundleIds)
		{
			bool isUnselecting = false;
			foreach (var id in bundleIds)
			{
				if (!selectedTargets.ContainsKey(id) || !chart.Contains(id)) continue;
				isUnselecting = true;
				SelectedTargets.Remove(id);
				if (CurrentSelecting != null && CurrentSelecting.Id == id)
				{
					CurrentSelecting = null;
				}
			}

			if (isUnselecting) EventManager.Instance.Invoke("Selection_OnUpdate");
			return isUnselecting;
		}

		public void UnselectAll()
		{
			if (selectedTargets.Count == 0) return;
			selectedTargets.Clear();
			CurrentSelecting = null;
			EventManager.Instance.Invoke("Selection_OnUpdate");
		}

		public void ToggleBundle(IEnumerable<int> bundleIds)
		{
			bool isUpdating = false;
			foreach (var id in bundleIds)
			{
				if (!chart.Contains(id)) continue;
				isUpdating = true;
				if (selectedTargets.ContainsKey(id))
				{
					selectedTargets.Remove(id);
					if (CurrentSelecting != null && CurrentSelecting.Id == id)
					{
						CurrentSelecting = null;
					}
				}
				else
				{
					selectedTargets.Add(id, chart[id]!);
					CurrentSelecting = chart[id]!;
				}
			}

			CurrentSelecting ??= selectedTargets.Values.FirstOrDefault();
			if (isUpdating) EventManager.Instance.Invoke("Selection_OnUpdate");
		}

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			chart = levelInfo.Chart;
			CurrentSelecting = null;
			if (selectedTargets.Count > 0)
			{
				selectedTargets.Clear();
				EventManager.Instance.Invoke("Selection_OnUpdate");
			}
		}

		private void ChartOnUpdate(ChartInfo chartInfo)
		{
			chart = chartInfo;
			var toRemove = SelectedTargets.Where(pair => !chart.Contains(pair.Key)).ToArray();
			bool currentSelectingExists = CurrentSelecting != null;
			if (toRemove.Length > 0)
			{
				foreach (var pair in toRemove)
				{
					selectedTargets.Remove(pair.Key);
					if (CurrentSelecting != null && pair.Key == CurrentSelecting.Id)
					{
						currentSelectingExists = false;
					}
				}

				if (!currentSelectingExists)
				{
					CurrentSelecting = selectedTargets.Values.FirstOrDefault();
				}

				EventManager.Instance.Invoke("Selection_OnUpdate");
			}
		}

		// System Functions
		void OnEnable()
		{
			ISelectManager.Instance = this;
			EventManager.Instance.AddListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
			EventManager.Instance.AddListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);
			foreach (var pair in registerSelectable)
			{
				// Currently not considering removing these components
				EventManager.Instance.AddListener(pair.Key, pair.Value);
			}
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
			EventManager.Instance.RemoveListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);
		}
	}
}