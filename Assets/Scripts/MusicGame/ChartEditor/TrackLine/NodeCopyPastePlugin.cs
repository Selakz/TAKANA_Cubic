using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Message;
using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Components;
using MusicGame.Components.Movement;
using MusicGame.Components.Tracks;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine
{
	public struct NodeCopyInfo : IEquatable<NodeCopyInfo>
	{
		public bool IsFirst { get; set; }

		public IMoveItem MoveItem { get; set; }

		public NodeCopyInfo(bool isFirst, IMoveItem moveItem)
		{
			IsFirst = isFirst;
			MoveItem = moveItem;
		}

		public bool Equals(NodeCopyInfo other)
		{
			return IsFirst == other.IsFirst && Equals(MoveItem, other.MoveItem);
		}

		public override bool Equals(object obj)
		{
			return obj is NodeCopyInfo other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(IsFirst, MoveItem);
		}
	}

	public class NodeCopyPastePlugin : MonoBehaviour
	{
		// Serializable and Public
		public IEnumerable<NodeCopyInfo> Clipboard => clipboard;

		public Track OriginalTrack { get; private set; }

		// Private
		private readonly List<NodeCopyInfo> clipboard = new();

		// Static

		// Defined Functions
		public void CopyToClipboard()
		{
			List<NodeCopyInfo> candidates = new();
			if (TrackMovementEditingManager.Instance.TryGetDecorator(
				    TrackMovementEditingManager.Instance.EditableDecorator, out var decorator))
			{
				if (decorator.MoveListDecorator1 != null)
				{
					candidates.AddRange(
						decorator.MoveListDecorator1.SelectedNodes.Select(n => new NodeCopyInfo(true, n.MoveItem)));
				}

				if (decorator.MoveListDecorator2 != null)
				{
					candidates.AddRange(
						decorator.MoveListDecorator2.SelectedNodes.Select(n => new NodeCopyInfo(false, n.MoveItem)));
				}
			}

			if (candidates.Count == 0) return;

			OriginalTrack = decorator.Model;
			HeaderMessage.Show("复制成功！", HeaderMessage.MessageType.Success);
			clipboard.Clear();
			clipboard.AddRange(candidates);

			EventManager.Instance.Invoke<object>("Edit_OnClearClipboard", this);
		}

		public void Paste()
		{
			if (!LevelManager.Instance.LevelCamera.ContainsScreenPoint(Input.mousePosition)) return;
			if (TrackMovementEditingManager.Instance.EditableDecorator == -1) return;

			if (clipboard.Count == 0)
			{
				return;
			}

			if (ISelectManager.Instance.CurrentSelecting is not EditingTrack editingTrack)
			{
				HeaderMessage.Show("需选中一条轨道以进行结点普通粘贴", HeaderMessage.MessageType.Info);
				return;
			}

			var track = editingTrack.Track;
			clipboard.Sort((a, b) => a.MoveItem.Time.CompareTo(b.MoveItem.Time));
			float baseTime = clipboard[0].MoveItem.Time;

			List<IUpdateMovementArg> cloneArgs = new();
			var mousePosition = Input.mousePosition;
			var gamePoint = LevelManager.Instance.LevelCamera.ScreenToWorldPoint(mousePosition);
			T3Time time = InScreenEditManager.Instance.TimeRetriever.GetTimeStart(gamePoint);

			foreach (var info in clipboard)
			{
				var newTime = time + info.MoveItem.Time - baseTime;
				if (newTime < track.TimeInstantiate || newTime > track.TimeEnd)
				{
					HeaderMessage.Show("粘贴片段长度超出轨道时间范围", HeaderMessage.MessageType.Info);
					return;
				}

				var newItem = info.MoveItem.SetTime(newTime);
				if (newItem is V1EMoveItem v1e)
				{
					var newX = track.Movement.GetPos(time) - OriginalTrack.Movement.GetPos(baseTime) + v1e.Position;
					newItem = new V1EMoveItem(v1e.Time, newX, v1e.Ease);
				}

				cloneArgs.Add(new UpdateMoveListArg(info.IsFirst, null, newItem));
			}

			var command = new UpdateMoveListCommand(cloneArgs);
			if (command.SetInit(track))
			{
				CommandManager.Instance.Add(command);
				HeaderMessage.Show("粘贴成功！", HeaderMessage.MessageType.Success);
			}
			else
			{
				HeaderMessage.Show("粘贴失败，与现有结点产生冲突", HeaderMessage.MessageType.Success);
			}
		}

		public void ExactPaste()
		{
			if (!LevelManager.Instance.LevelCamera.ContainsScreenPoint(Input.mousePosition)) return;
			if (TrackMovementEditingManager.Instance.EditableDecorator == -1) return;

			if (clipboard.Count == 0)
			{
				return;
			}

			if (ISelectManager.Instance.CurrentSelecting is not EditingTrack editingTrack)
			{
				HeaderMessage.Show("需选中一条轨道以进行结点普通粘贴", HeaderMessage.MessageType.Info);
				return;
			}

			var track = editingTrack.Track;
			clipboard.Sort((a, b) => a.MoveItem.Time.CompareTo(b.MoveItem.Time));
			float baseTime = clipboard[0].MoveItem.Time;

			List<IUpdateMovementArg> cloneArgs = new();
			var mousePosition = Input.mousePosition;
			var gamePoint = LevelManager.Instance.LevelCamera.ScreenToWorldPoint(mousePosition);
			T3Time time = InScreenEditManager.Instance.TimeRetriever.GetTimeStart(gamePoint);
			foreach (var info in clipboard)
			{
				var newTime = time + info.MoveItem.Time - baseTime;
				if (newTime < track.TimeInstantiate || newTime > track.TimeEnd)
				{
					HeaderMessage.Show("粘贴片段长度超出轨道时间范围", HeaderMessage.MessageType.Info);
					return;
				}

				cloneArgs.Add(new UpdateMoveListArg(info.IsFirst, null, info.MoveItem.SetTime(newTime)));
			}

			var command = new UpdateMoveListCommand(cloneArgs);
			if (command.SetInit(track))
			{
				CommandManager.Instance.Add(command);
				HeaderMessage.Show("粘贴成功！", HeaderMessage.MessageType.Success);
			}
			else
			{
				HeaderMessage.Show("粘贴失败，与现有结点产生冲突", HeaderMessage.MessageType.Success);
			}
		}

		public void CheckClipboard()
		{
			if (clipboard.Count == 0) return;
			HeaderMessage.Show($"已复制{clipboard.Count}个结点", HeaderMessage.MessageType.Info);
		}

		// Event Handlers
		private void OnClearClipboard(object sender)
		{
			if (ReferenceEquals(sender, this)) return;
			clipboard.Clear();
		}

		private void VetoEditQueryCopy(VetoArg arg, IComponent toCopyComponent)
		{
			if (TrackMovementEditingManager.Instance.TryGetDecorator(toCopyComponent.Id, out var decorator) &&
			    decorator.IsEditing)
			{
				arg.Veto();
			}
		}

		private void VetoEditQueryPaste(VetoArg arg)
		{
			if (clipboard.Count > 0)
			{
				arg.Veto();
			}
		}

		private void VetoEditQueryCheckClipboard(VetoArg arg)
		{
			if (clipboard.Count > 0)
			{
				arg.Veto();
			}
		}

		// System Functions
		void OnEnable()
		{
			EventManager.Instance.AddListener<object>("Edit_OnClearClipboard", OnClearClipboard);
			EventManager.Instance.AddVetoListener<IComponent>("Edit_QueryCopy", VetoEditQueryCopy);
			EventManager.Instance.AddVetoListener("Edit_QueryPaste", VetoEditQueryPaste);
			EventManager.Instance.AddVetoListener("Edit_QueryCheckClipboard", VetoEditQueryCheckClipboard);

			InputManager.Instance.Register("InScreenEdit", "Copy", _ => CopyToClipboard());
			InputManager.Instance.Register("InScreenEdit", "Paste", _ => Paste());
			InputManager.Instance.Register("InScreenEdit", "ExactPaste", _ => ExactPaste());
			InputManager.Instance.Register("InScreenEdit", "CheckClipboard", _ => CheckClipboard());
		}

		private void OnDisable()
		{
			EventManager.Instance.RemoveListener<object>("Edit_OnClearClipboard", OnClearClipboard);
			EventManager.Instance.RemoveVetoListener<IComponent>("Edit_QueryCopy", VetoEditQueryCopy);
			EventManager.Instance.RemoveVetoListener("Edit_QueryPaste", VetoEditQueryPaste);
			EventManager.Instance.RemoveVetoListener("Edit_QueryCheckClipboard", VetoEditQueryCheckClipboard);
		}
	}
}