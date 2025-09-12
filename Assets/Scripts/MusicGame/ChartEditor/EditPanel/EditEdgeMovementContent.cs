using System;
using System.Collections.Generic;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.Message;
using MusicGame.Components.Movement;
using MusicGame.Components.Tracks;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.EditPanel
{
	public class EditEdgeMovementContent : MonoBehaviour, IEditTrackMovementContent
	{
		// Serializable and Public
		[SerializeField] private Toggle leftToggle;
		[SerializeField] private Toggle rightToggle;
		[SerializeField] private Transform movementContentRoot;

		public EditingTrack Model
		{
			get => model;
			set
			{
				Awake();
				model = value;
				SetMovements();
				transform.localScale = Vector3.one;
			}
		}

		// Private
		private EditingTrack model;
		private IEditMovementContent leftContent;
		private IEditMovementContent rightContent;
		private bool hasAwaken = false;

		// Static
		private static Dictionary<Type, LazyPrefab> movementContentPrefabs;
		private static Dictionary<Type, Type> movementTypeMap;

		// Defined Functions
		private void SetMovements()
		{
			var leftMovementType = Model.Track.Movement.Movement1.GetType();
			if (leftContent == null)
			{
				if (movementContentPrefabs.TryGetValue(leftMovementType, out var prefab))
				{
					var go = prefab.Instantiate(movementContentRoot);
					go.transform.SetSiblingIndex(0);
					leftContent = go.GetComponent<IEditMovementContent>();
				}
			}
			else
			{
				leftContent.OnMovementUpdated -= DoCommand;
				if (!movementTypeMap.TryGetValue(leftMovementType, out var contentType) ||
				    leftContent.GetType() != contentType)
				{
					Destroy(((MonoBehaviour)leftContent).gameObject);
					if (movementContentPrefabs.TryGetValue(leftMovementType, out var prefab))
					{
						var go = prefab.Instantiate(movementContentRoot);
						go.transform.SetSiblingIndex(0);
						leftContent = go.GetComponent<IEditMovementContent>();
					}
				}
			}

			var rightMovementType = Model.Track.Movement.Movement2.GetType();
			if (rightContent == null)
			{
				if (movementContentPrefabs.TryGetValue(rightMovementType, out var prefab))
				{
					var go = prefab.Instantiate(movementContentRoot);
					go.SetActive(false);
					go.transform.SetSiblingIndex(1);
					rightContent = go.GetComponent<IEditMovementContent>();
				}
			}
			else
			{
				rightContent.OnMovementUpdated -= DoCommand;
				if (!movementTypeMap.TryGetValue(rightMovementType, out var contentType) ||
				    rightContent.GetType() != contentType)
				{
					Destroy(((MonoBehaviour)rightContent).gameObject);
					if (movementContentPrefabs.TryGetValue(rightMovementType, out var prefab))
					{
						var go = prefab.Instantiate(movementContentRoot);
						go.transform.SetSiblingIndex(1);
						go.SetActive(false);
						rightContent = go.GetComponent<IEditMovementContent>();
					}
				}
			}

			if (leftContent != null)
			{
				leftContent.Movement = Model.Track.Movement.Movement1;
				leftContent.IsFirst = true;
				leftContent.OnMovementUpdated += DoCommand;
			}

			if (rightContent != null)
			{
				rightContent.Movement = Model.Track.Movement.Movement2;
				rightContent.IsFirst = false;
				rightContent.OnMovementUpdated += DoCommand;
			}
		}

		// Event Handlers
		private void OnLeftToggleValueChanged(bool isOn)
		{
			if (!isOn) return;
			((MonoBehaviour)leftContent).gameObject.SetActive(true);
			((MonoBehaviour)rightContent).gameObject.SetActive(false);
		}

		private void OnRightToggleValueChanged(bool isOn)
		{
			if (!isOn) return;
			((MonoBehaviour)leftContent).gameObject.SetActive(false);
			((MonoBehaviour)rightContent).gameObject.SetActive(true);
		}

		private void DoCommand(ISetInitCommand<Track> command)
		{
			if (command.SetInit(Model.Track))
			{
				Debug.Log("do command");
				CommandManager.Instance.Add(command);
			}
			else
			{
				Debug.Log("fail to command");
				HeaderMessage.Show("ÐÞ¸ÄÊ§°Ü", HeaderMessage.MessageType.Warn);
			}
		}

		// System Functions
		void Awake()
		{
			if (hasAwaken) return;
			hasAwaken = true;

			leftToggle.onValueChanged.AddListener(OnLeftToggleValueChanged);
			rightToggle.onValueChanged.AddListener(OnRightToggleValueChanged);
			movementContentPrefabs ??= new()
			{
				[typeof(V1EMoveList)] =
					new LazyPrefab("Prefabs/EditorUI/EditPanel/EditMoveListContent", "EditMoveListContentPrefab_OnLoad")
			};
			movementTypeMap ??= new()
			{
				[typeof(V1EMoveList)] = typeof(EditMoveListContent)
			};
		}
	}
}