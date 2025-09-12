using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.Select;
using MusicGame.Components.Chart;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.ListRender;
using TMPro;
using UnityEngine;

namespace MusicGame.ChartEditor.EditPanel
{
	[RequireComponent(typeof(ListRendererInt))]
	public class EditPanelManager : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private TMP_Text selectTitleText;

		public ListRendererInt ListRenderer { get; private set; }

		// Private
		private Dictionary<Type, Type> editTypeMap;

		// Static

		// Defined Functions
		public void RenderTitle()
		{
			int count = ISelectManager.Instance.SelectedTargets.Count;
			selectTitleText.text = $"选中了{count}个元件";
		}

		public void RenderContent()
		{
			HashSet<int> selectedComponents = new(ISelectManager.Instance.SelectedTargets.Keys);
			var enumerate = ListRenderer.Values.ToList();
			foreach (var child in enumerate)
			{
				var content = child.GetComponent<IEditComponentContent>();
				var id = content.Model.Id;
				if (!ISelectManager.Instance.IsSelected(id)) ListRenderer.Remove(id);
				else selectedComponents.Remove(id);
			}

			foreach (var id in selectedComponents)
			{
				var component = ISelectManager.Instance.SelectedTargets[id];
				if (component is not EditingComponent editingComponent)
				{
					Debug.LogError($"Do not receive {nameof(EditingComponent)} from {nameof(ISelectManager)}");
					continue;
				}

				foreach (var typePair in editTypeMap)
				{
					if (typePair.Key.IsInstanceOfType(editingComponent))
					{
						var script = ListRenderer.Add(typePair.Value, id);
						if (script is IEditComponentContent content)
						{
							content.Model = editingComponent;
						}

						break;
					}
				}
			}
		}

		// Event Handlers
		private void SelectionOnUpdate()
		{
			RenderTitle();
			RenderContent();
		}

		private void ChartOnUpdate(ChartInfo chartInfo)
		{
			var enumerate = ListRenderer.Values.ToList();
			foreach (var child in enumerate)
			{
				IEditComponentContent content = child.GetComponent<IEditComponentContent>();
				if (chartInfo.Contains(content.Model.Id))
				{
					content.Model = content.Model; // Refresh
				}
				else
				{
					ListRenderer.Remove(content.Model.Id);
				}
			}
		}

		// System Functions
		void Awake()
		{
			editTypeMap = new()
			{
				[typeof(EditingTrack)] = typeof(EditTrackContent),
				[typeof(EditingHold)] = typeof(EditHoldContent),
				[typeof(EditingNote)] = typeof(EditNoteContent)
			};
			Dictionary<Type, LazyPrefab> listPrefabs = new()
			{
				[typeof(EditTrackContent)] =
					new LazyPrefab("Prefabs/EditorUI/EditPanel/EditTrackContent", "EditTrackContentPrefab_OnLoad"),
				[typeof(EditHoldContent)] =
					new LazyPrefab("Prefabs/EditorUI/EditPanel/EditHoldContent", "EditHoldContentPrefab_OnLoad"),
				[typeof(EditNoteContent)] =
					new LazyPrefab("Prefabs/EditorUI/EditPanel/EditNoteContent", "EditNoteContentPrefab_OnLoad"),
			};
			ListRenderer = GetComponent<ListRendererInt>();
			ListRenderer.Init(listPrefabs);
			ListRenderer.ListSorter = (a, b) => a.CompareTo(b);
		}

		void OnEnable()
		{
			EventManager.Instance.AddListener("Selection_OnUpdate", SelectionOnUpdate);
			EventManager.Instance.AddListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener("Selection_OnUpdate", SelectionOnUpdate);
			EventManager.Instance.RemoveListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);
		}
	}
}