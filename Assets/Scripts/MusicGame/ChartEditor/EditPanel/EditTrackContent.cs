using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.EditPanel.Commands;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Level;
using MusicGame.ChartEditor.Message;
using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLayer;
using MusicGame.Components.Tracks.Movement;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.MVC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.EditPanel
{
	public class EditTrackContent : MonoBehaviour, IController<EditingTrack>, IEditComponentContent
	{
		// Serializable and Public
		[SerializeField] private EditNameTitle nameTitle;
		[SerializeField] private Button unselectButton;
		[SerializeField] private Button deleteButton;
		[SerializeField] private Transform contentRoot;
		[SerializeField] private TMP_InputField timeStartInputField;
		[SerializeField] private TMP_InputField timeEndInputField;
		[SerializeField] private TMP_Dropdown layerDropdown;
		[SerializeField] private float heightIncrement;

		public IModel GenericModel => Model;

		public EditingTrack Model
		{
			get => model;
			set
			{
				Awake();
				model = value;
				nameTitle.Model = model;
				timeStartInputField.SetTextWithoutNotify(model.Track.TimeInstantiate.ToString());
				timeEndInputField.SetTextWithoutNotify(model.Track.TimeEnd.ToString());
				SetLayer();
				SetMovement();
				transform.localScale = Vector3.one;
			}
		}

		EditingComponent IEditComponentContent.Model
		{
			get => Model;
			set
			{
				if (value is not EditingTrack track)
				{
					Debug.LogError($"EditTrackContent.Model only receives {nameof(EditingTrack)}");
					return;
				}

				Model = track;
			}
		}

		public GameObject Object => gameObject;

		// Private
		private EditingTrack model;
		private IEditTrackMovementContent trackMovementContent;
		private int[] layerIds;

		// Static
		private static Dictionary<Type, LazyPrefab> movementContentPrefabs;
		private static Dictionary<Type, Type> movementTypeMap;

		// Defined Functions
		private void SetLayer()
		{
			layerIds = layerDropdown.SetOptions(
				TrackLayerManager.Instance.ListRenderer.Keys.ToList(),
				id =>
				{
					TrackLayerManager.Instance.TryGetLayer(id, out var layerInfo);
					return layerInfo.Name;
				});
			var layer = Model.Properties.Get("layer", TrackLayerManager.Instance.FallbackLayerInfo.Id);
			for (var i = 0; i < layerIds.Length; i++)
			{
				var layerId = layerIds[i];
				if (layer == layerId)
				{
					layerDropdown.SetValueWithoutNotify(i);
				}
			}
		}

		private void SetMovement()
		{
			var movementType = Model.Track.Movement.GetType();
			if (trackMovementContent == null)
			{
				if (movementContentPrefabs.TryGetValue(movementType, out var prefab))
				{
					var go = prefab.Instantiate(contentRoot);
					go.transform.SetSiblingIndex(1);
					trackMovementContent = go.GetComponent<IEditTrackMovementContent>();
					trackMovementContent.Model = Model;
				}
			}
			else
			{
				if (movementTypeMap.TryGetValue(movementType, out var contentType) &&
				    trackMovementContent.GetType() == contentType)
				{
					trackMovementContent.Model = Model;
				}
				else
				{
					Destroy(((MonoBehaviour)trackMovementContent).gameObject);
					if (movementContentPrefabs.TryGetValue(movementType, out var prefab))
					{
						var go = prefab.Instantiate(contentRoot);
						go.transform.SetSiblingIndex(1);
						trackMovementContent = go.GetComponent<IEditTrackMovementContent>();
						trackMovementContent.Model = Model;
					}
				}
			}
		}

		public void Init(EditingTrack model)
		{
			Model = model;
		}

		public void Destroy()
		{
			// Released to pool
			Object.SetActive(false);
		}

		// Event Handlers
		private void OnUnselectButtonPressed()
		{
			ISelectManager.Instance.Unselect(Model.Id);
		}

		private void OnDeleteButtonPressed()
		{
			var command = new DeleteComponentsCommand(Model);
			command.OnRedo += () =>
			{
				ISelectManager.Instance.UnselectAll();
				ISelectManager.Instance.Select(Model.Id);
			};
			CommandManager.Instance.Add(command);
		}

		private void OnTimeStartInputFieldEndEdit(string content)
		{
			// TODO: Replace with new TimeStart
			if (int.TryParse(content, out int newStart) && newStart < Model.TimeEnd &&
			    IEditingChartManager.Instance.GetChildrenComponents(Model.Id)
				    .All(c => c is not EditingNote e || e.Note.TimeJudge >= newStart))
			{
				if (newStart == Model.Track.TimeInstantiate) return;
				var command = new UpdateTrackTimeStartCommand(Model.Track, newStart);
				CommandManager.Instance.Add(command);
			}
			else
			{
				HeaderMessage.Show("ÐÞ¸ÄÊ§°Ü", HeaderMessage.MessageType.Warn);
				timeStartInputField.SetTextWithoutNotify(Model.Track.TimeInstantiate.ToString());
			}
		}

		private void OnTimeEndInputFieldEndEdit(string content)
		{
			if (int.TryParse(content, out int newEnd) && newEnd > Model.TimeInstantiate &&
			    IEditingChartManager.Instance.GetChildrenComponents(Model.Id).All(c => c.TimeEnd <= newEnd))
			{
				if (newEnd == Model.Track.TimeEnd) return;
				var command = new UpdateTrackTimeEndCommand(Model.Track, newEnd);
				CommandManager.Instance.Add(command);
			}
			else
			{
				HeaderMessage.Show("ÐÞ¸ÄÊ§°Ü", HeaderMessage.MessageType.Warn);
				timeStartInputField.SetTextWithoutNotify(Model.Track.TimeInstantiate.ToString());
			}
		}

		private void OnLayerDropdownChanged(int newValue)
		{
			if (!EventManager.Instance.InvokeVeto("Layer_QueryTrackLayerChange",
				    Model, layerIds[newValue], out var reasons))
			{
				HeaderMessage.Show(reasons.First(), HeaderMessage.MessageType.Warn);
				return;
			}

			var oldLayer = Model.Properties.Get("layer", TrackLayerManager.Instance.FallbackLayerInfo.Id);
			CommandManager.Instance.Add(new UpdateComponentsCommand(new UpdateComponentArg(
				Model,
				model => model.Properties["layer"] = layerIds[newValue],
				model => model.Properties["layer"] = oldLayer
			)));
		}

		// System Functions
		private bool hasAwaken = false;

		void Awake()
		{
			// TODO: Replace with T3MonoBehaviour.EarlyAwake
			if (hasAwaken) return;
			hasAwaken = true;

			movementContentPrefabs ??= new()
			{
				[typeof(TrackEdgeMovement)] = new LazyPrefab("Prefabs/EditorUI/EditPanel/EditEdgeMovementContent",
					"EditEdgeMovementContent_OnLoad")
			};
			movementTypeMap ??= new()
			{
				[typeof(TrackEdgeMovement)] = typeof(EditEdgeMovementContent)
			};

			unselectButton.onClick.AddListener(OnUnselectButtonPressed);
			deleteButton.onClick.AddListener(OnDeleteButtonPressed);
			timeStartInputField.onEndEdit.AddListener(OnTimeStartInputFieldEndEdit);
			timeEndInputField.onEndEdit.AddListener(OnTimeEndInputFieldEndEdit);
			layerDropdown.onValueChanged.AddListener(OnLayerDropdownChanged);
		}
	}
}