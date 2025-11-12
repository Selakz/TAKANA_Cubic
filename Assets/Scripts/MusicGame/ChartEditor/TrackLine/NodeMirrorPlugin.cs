#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.Message;
using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Components.Movement;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine
{
	public class NodeMirrorPlugin : MonoBehaviour
	{
		// Static
		private static IMoveItem Mirror(V1EMoveItem moveItem)
		{
			return new V1EMoveItem(moveItem.Time, -moveItem.Position, moveItem.Ease);
		}

		// Event Handlers
		private void NodeMirror()
		{
			List<IUpdateMovementArg> mirrorArgs = new();
			if (TrackMovementEditingManager.Instance.TryGetDecorator(
				    TrackMovementEditingManager.Instance.EditableDecorator, out var decorator))
			{
				if (decorator.MoveListDecorator1 != null)
				{
					foreach (var node in decorator.MoveListDecorator1.SelectedNodes)
					{
						if (node.MoveItem is V1EMoveItem moveItem)
						{
							var newItem = Mirror(moveItem);
							mirrorArgs.Add(new UpdateMoveListArg(true, moveItem, newItem));
						}
					}
				}

				if (decorator.MoveListDecorator2 != null)
				{
					foreach (var node in decorator.MoveListDecorator2.SelectedNodes)
					{
						if (node.MoveItem is V1EMoveItem moveItem)
						{
							var newItem = Mirror(moveItem);
							mirrorArgs.Add(new UpdateMoveListArg(false, moveItem, newItem));
						}
					}
				}
			}

			if (mirrorArgs.Count == 0) return;
			if (ISelectManager.Instance.CurrentSelecting is not EditingTrack editingTrack) return;
			var track = editingTrack.Track;

			var command = new UpdateMoveListCommand(mirrorArgs);
			if (command.SetInit(track))
			{
				CommandManager.Instance.Add(command);
			}
			else
			{
				HeaderMessage.Show("¾µÏñÊ§°Ü", HeaderMessage.MessageType.Error);
			}
		}

		private void EditQueryMirror(VetoArg arg)
		{
			if (TrackMovementEditingManager.Instance.TryGetDecorator(
				    TrackMovementEditingManager.Instance.EditableDecorator, out var decorator))
			{
				if (decorator.MoveListDecorator1 != null && decorator.MoveListDecorator1.SelectedNodes.Any())
				{
					arg.Veto();
				}

				else if (decorator.MoveListDecorator2 != null && decorator.MoveListDecorator2.SelectedNodes.Any())
				{
					arg.Veto();
				}
			}
		}

		// System Functions
		void OnEnable()
		{
			EventManager.Instance.AddVetoListener("Edit_QueryMirror", EditQueryMirror);

			InputManager.Instance.Register("InScreenEdit", "Mirror", _ => NodeMirror());
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveVetoListener("Edit_QueryMirror", EditQueryMirror);
		}
	}
}