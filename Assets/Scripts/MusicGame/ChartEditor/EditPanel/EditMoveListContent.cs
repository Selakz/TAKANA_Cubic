using System;
using System.Collections.Generic;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Components.Movement;
using MusicGame.Components.Tracks;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.ListRender;
using UnityEngine;

namespace MusicGame.ChartEditor.EditPanel
{
	public class EditMoveListContent : MonoBehaviour, IEditMovementContent
	{
		// Serializable and Public
		[SerializeField] private ListRendererInt listRenderer;

		public event Action<ISetInitCommand<Track>> OnMovementUpdated;

		public ListRendererInt ListRenderer => listRenderer;

		public IMoveList MoveList
		{
			get => moveList;
			set
			{
				// It's so fantastic that you can't guarantee when will Awake() be called haha
				if (!hasAwaken) Awake();
				moveList = value;
				foreach (var go in ListRenderer.Values)
				{
					if (go.TryGetComponent<IEditMoveItemContent>(out var content))
					{
						content.OnMoveItemUpdated -= OnMoveItemUpdated;
					}
				}

				ListRenderer.Clear();
				foreach (var moveItem in moveList)
				{
					var script = ListRenderer.Add(editTypeMap[moveItem.GetType()], moveItem.Time);
					if (script is IEditMoveItemContent editMoveItemContent)
					{
						editMoveItemContent.MoveItem = moveItem;
						editMoveItemContent.OnMoveItemUpdated += OnMoveItemUpdated;
					}
				}

				transform.localScale = Vector3.one;
			}
		}

		public bool IsFirst { get; set; }

		public IMovement<float> Movement
		{
			get => (IMovement<float>)MoveList;
			set
			{
				if (value is not IMoveList moveListValue)
				{
					Debug.LogError($"EditMoveListContent can't set movement not of type {nameof(IMoveList)}");
					return;
				}

				MoveList = moveListValue;
			}
		}

		// Private
		private static Dictionary<Type, Type> editTypeMap;
		private IMoveList moveList;

		// Event Handlers
		private void OnMoveItemUpdated(IMoveItem oldItem, IMoveItem newItem)
		{
			var command = new UpdateMoveListCommand(new[] { new UpdateMoveListArg(IsFirst, oldItem, newItem) });
			OnMovementUpdated?.Invoke(command);
		}

		// System Functions
		private bool hasAwaken = false;

		void Awake()
		{
			if (hasAwaken) return;
			hasAwaken = true;
			editTypeMap ??= new()
			{
				[typeof(V1EMoveItem)] = typeof(EditV1EItemContent)
			};
			Dictionary<Type, LazyPrefab> listPrefabs = new()
			{
				[typeof(EditV1EItemContent)] =
					new LazyPrefab("Prefabs/EditorUI/EditPanel/EditV1EItemContent", "EditV1EItemContentPrefab_OnLoad")
			};
			ListRenderer.Init(listPrefabs);
			ListRenderer.ListSorter = (a, b) => a.CompareTo(b);
		}
	}
}