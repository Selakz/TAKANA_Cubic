using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLine.TrackDecorator;
using MusicGame.Components;
using MusicGame.Components.Chart;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using T3Framework.Static.Easing;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine
{
	public class TrackMovementEditingManager : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Transform decoratorRoot;

		public static TrackMovementEditingManager Instance { get; private set; }

		public int CurrentEaseId { get; set; } = 1;

		public int EditableDecorator { get; private set; } = -1;

		// Private
		private readonly Dictionary<int, ITrackDecorator> decorators = new();
		private readonly HashSet<int> enabledDecorators = new();
		private bool isProtecting = false;

		// Static
		public Eases GetNextEase(Eases ease)
		{
			var label = ease.GetString();
			var newLabel = label switch
			{
				"u" => "s",
				"s" => CurveCalculator.GetEaseById(CurrentEaseId * 10 + 2).GetString(),
				_ => label[1] switch
				{
					'i' => label[0] + "o",
					'o' => label[0] + "b",
					'b' => label[0] + "a",
					'a' => "u",
					_ => label switch
					{
						"u" => "s",
						"s" => "si",
						"si" => "so",
						"so" => "sb",
						"sb" => "sa",
						"sa" => "u",
						_ => "u"
					}
				}
			};
			return CurveCalculator.GetEaseByName(newLabel);
		}

		// Defined Functions
		public bool TryGetDecorator(int id, out ITrackDecorator decorator)
		{
			return decorators.TryGetValue(id, out decorator);
		}

		private IEnumerator ProtectOneFrame()
		{
			if (EditableDecorator == -1 || !decorators[EditableDecorator].IsEditing) yield break;
			isProtecting = true;
			yield return null;
			isProtecting = false;
		}

		// Event Handlers
		private void SelectionOnUpdate()
		{
			var toRemoves = enabledDecorators.Where(
				enabledDecorator => !ISelectManager.Instance.SelectedTargets.ContainsKey(enabledDecorator)).ToArray();
			foreach (var toRemove in toRemoves)
			{
				decorators[toRemove].Disable();
				enabledDecorators.Remove(toRemove);
			}

			foreach (var selecting in ISelectManager.Instance.SelectedTargets.Values)
			{
				if (selecting is EditingTrack editingTrack)
				{
					if (!decorators.ContainsKey(editingTrack.Id))
					{
						var decorator = ITrackDecorator.Decorate(decoratorRoot, editingTrack.Track);
						decorators.Add(editingTrack.Id, decorator);
					}

					decorators[editingTrack.Id].Enable();
					enabledDecorators.Add(editingTrack.Id);
				}
			}

			if (ISelectManager.Instance.CurrentSelecting is EditingTrack selectingTrack &&
			    selectingTrack.Id != EditableDecorator)
			{
				if (EditableDecorator != -1) decorators[EditableDecorator].Uneditable();
				decorators[selectingTrack.Id].Editable();
				EditableDecorator = selectingTrack.Id;
			}
			else if (ISelectManager.Instance.CurrentSelecting is null)
			{
				EditableDecorator = -1;
			}
		}

		private void ChartOnUpdate(ChartInfo chartInfo)
		{
			var toRemoves = enabledDecorators.Where(
				enabledDecorator => !chartInfo.Contains(enabledDecorator)).ToArray();
			foreach (var toRemove in toRemoves)
			{
				decorators[toRemove].Destroy();
				decorators.Remove(toRemove);
				enabledDecorators.Remove(toRemove);

				if (toRemove == EditableDecorator)
				{
					EditableDecorator = -1;
				}
			}
		}

		private void DoCommand(Func<ITrackDecorator, ICommand> func, bool isNotesSensitive)
		{
			if (EditableDecorator == -1 ||
			    (isNotesSensitive && ISelectManager.Instance.SelectedTargets.Values.OfType<EditingNote>().Any()))
				return;

			CommandManager.Instance.Add(func(decorators[EditableDecorator]));
		}

		private void OnRaycastNode(bool unselectAllBeforeRaycast)
		{
			if (unselectAllBeforeRaycast && EditableDecorator != -1) decorators[EditableDecorator].UnselectAll();
			var screenPoint = Input.mousePosition;
			if (!LevelManager.Instance.LevelCamera.ContainsScreenPoint(screenPoint)) return;
			Ray ray = LevelManager.Instance.LevelCamera.ScreenPointToRay(screenPoint);
			var layerMask = LayerMask.GetMask("Nodes");
			var hit = Physics2D.Raycast(ray.origin, ray.direction, float.MaxValue, layerMask);
			if (hit.transform != null)
			{
				var node = hit.transform.parent.GetComponent<IMovementNode>();
				node.IsSelected = true;
			}
		}

		private void VetoEditQueryDelete(VetoArg arg, IComponent toDeleteComponent)
		{
			if (toDeleteComponent.Id == EditableDecorator &&
			    (decorators[EditableDecorator].IsEditing || isProtecting))
			{
				arg.Veto();
			}
		}

		// System Functions
		void Awake()
		{
			Instance = this;
		}

		void OnEnable()
		{
			EventManager.Instance.AddListener("Selection_OnUpdate", SelectionOnUpdate);
			EventManager.Instance.AddListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);
			EventManager.Instance.AddVetoListener<IComponent>("Edit_QueryDelete", VetoEditQueryDelete);

			InputManager.Instance.Register("InScreenEdit", "ToLeft",
				_ => DoCommand(decorator => decorator.ToLeft(), false));
			InputManager.Instance.Register("InScreenEdit", "ToRight",
				_ => DoCommand(decorator => decorator.ToRight(), false));
			InputManager.Instance.Register("InScreenEdit", "ToLeftGrid",
				_ => DoCommand(decorator => decorator.ToLeftGrid(), false));
			InputManager.Instance.Register("InScreenEdit", "ToRightGrid",
				_ => DoCommand(decorator => decorator.ToRightGrid(), false));
			InputManager.Instance.Register("InScreenEdit", "ToNext",
				_ => DoCommand(decorator => decorator.ToNext(), true));
			InputManager.Instance.Register("InScreenEdit", "ToPrevious",
				_ => DoCommand(decorator => decorator.ToPrevious(), true));
			InputManager.Instance.Register("InScreenEdit", "ToNextBeat",
				_ => DoCommand(decorator => decorator.ToNextBeat(), true));
			InputManager.Instance.Register("InScreenEdit", "ToPreviousBeat",
				_ => DoCommand(decorator => decorator.ToPreviousBeat(), true));
			InputManager.Instance.Register("InScreenEdit", "CreateNode",
				_ => DoCommand(decorator => decorator.Create
					(LevelManager.Instance.LevelCamera.ScreenToWorldPoint(Input.mousePosition)), false));
			InputManager.Instance.Register("InScreenEdit", "Delete",
				_ =>
				{
					StartCoroutine(ProtectOneFrame());
					DoCommand(decorator => decorator.Delete(), false);
				});
			InputManager.Instance.Register("CurveSwitch", "ChangeNodeState",
				_ => DoCommand(decorator => decorator.ChangeNodeState(), false));

			InputManager.Instance.Register("InScreenEdit", "RaycastNode", _ => OnRaycastNode(true));
			InputManager.Instance.Register("InScreenEdit", "RaycastNodeMulti", _ => OnRaycastNode(false));
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener("Selection_OnUpdate", SelectionOnUpdate);
			EventManager.Instance.RemoveListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);
			EventManager.Instance.RemoveVetoListener<IComponent>("Edit_QueryDelete", VetoEditQueryDelete);
		}
	}
}