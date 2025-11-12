using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Components.Movement;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Static.Easing;
using UnityEngine;
using UnityEngine.Pool;

namespace MusicGame.ChartEditor.TrackLine.MoveListDecorator
{
	public class V1EDecorator : MonoBehaviour, IMoveListDecorator
	{
		// Serializable and Public
		public bool IsEditable
		{
			get => isEditable;
			set
			{
				if (isEditable == value) return;
				isEditable = value;
				foreach (var node in nodes.Values)
				{
					node.IsEditable = value;
					if (!value) node.IsSelected = false;
				}
			}
		}

		public IMovement<float> Movement => moveList;

		public IEnumerable<IMovementNode> SelectedNodes => nodes.Values.Where(node => node.IsSelected);

		public bool IsFirst
		{
			get => isFirst;
			set
			{
				isFirst = value;
				// TODO: Change line color
			}
		}

		// Private
		private T3Time timeTrackStart;
		private float speedRate;
		private V1EMoveList moveList;
		private readonly Dictionary<T3Time, V1ENode> nodes = new();

		private bool isFirst;
		private bool isEditable;

		private readonly ObjectPool<V1ENode> nodePool = new(
			() => Instantiate<GameObject>(lazyPrefab).GetComponent<V1ENode>(),
			node => node.gameObject.SetActive(true),
			node => node.gameObject.SetActive(false),
			node => Destroy(node.gameObject)
		);

		// Static
		private static readonly LazyPrefab lazyPrefab =
			new("Prefabs/EditorUI/TrackLine/V1ENode", "V1ENodePrefab_OnLoad");

		// Defined Functions
		public void Init(bool isFirst, T3Time timeTrackStart, IMovement<float> moveList, float speedRate)
		{
			IsFirst = isFirst;
			this.timeTrackStart = timeTrackStart;
			this.speedRate = speedRate;
			this.moveList = moveList as V1EMoveList;
			Refresh();
		}

		public void Refresh()
		{
			var toRemove = nodes.Where(pair => !moveList.TryGet(pair.Key, out _)).ToArray();
			foreach (var pair in toRemove)
			{
				nodePool.Release(pair.Value);
				nodes.Remove(pair.Key);
			}

			V1LSMoveList aligner = new V1LSMoveList(timeTrackStart, 0, speedRate);
			V1EMoveItem? next = null;
			foreach (var item in moveList)
			{
				var current = next;
				next = item;
				if (current != null)
				{
					Vector2 currentPos = new(current.Value.Position, aligner.GetPos(current.Value.Time));
					Vector2 nextPos = new(next.Value.Position, aligner.GetPos(next.Value.Time));
					V1ENode node = nodes.TryGetValue(current.Value.Time, out var existNode)
						? existNode
						: nodePool.Get();
					node.transform.SetParent(transform);
					node.IsEditable = IsEditable;
					node.Init(current.Value, currentPos, nextPos);
					nodes[current.Value.Time] = node;
				}
			}

			if (next != null)
			{
				Vector2 lastPos = new(next.Value.Position, aligner.GetPos(next.Value.Time));
				V1ENode node = nodes.TryGetValue(next.Value.Time, out var existNode)
					? existNode
					: nodePool.Get();
				node.transform.SetParent(transform);
				node.IsEditable = IsEditable;
				node.Init(next.Value, lastPos, lastPos);
				nodes[next.Value.Time] = node;
			}
		}

		public void UnselectAll()
		{
			foreach (var node in nodes.Values) node.IsSelected = false;
		}

		public void Destroy()
		{
			Destroy(gameObject);
		}

		private IEnumerable<UpdateMoveListArg> To(Func<IMovementNode, IMoveItem> func)
		{
			return from node in SelectedNodes
				let oldMoveItem = (node.MoveItem as V1EMoveItem?)!.Value
				let newMoveItem = (func(node) as V1EMoveItem?)!.Value
				select new UpdateMoveListArg(IsFirst, oldMoveItem, newMoveItem);
		}

		public IEnumerable<IUpdateMovementArg> ToLeft()
		{
			return To(node => node.ToLeft());
		}

		public IEnumerable<IUpdateMovementArg> ToRight()
		{
			return To(node => node.ToRight());
		}

		public IEnumerable<IUpdateMovementArg> ToLeftGrid()
		{
			return To(node => node.ToLeftGrid());
		}

		public IEnumerable<IUpdateMovementArg> ToRightGrid()
		{
			return To(node => node.ToRightGrid());
		}

		public IEnumerable<IUpdateMovementArg> ToNext()
		{
			return To(node => node.ToNext());
		}

		public IEnumerable<IUpdateMovementArg> ToPrevious()
		{
			return To(node => node.ToPrevious());
		}

		public IEnumerable<IUpdateMovementArg> ToNextBeat()
		{
			return InScreenEditManager.Instance.TimeRetriever is not GridTimeRetriever
				? Enumerable.Empty<IUpdateMovementArg>()
				: To(node => node.ToNextBeat());
		}

		public IEnumerable<IUpdateMovementArg> ToPreviousBeat()
		{
			return To(node => node.ToPreviousBeat());
		}

		public IEnumerable<IUpdateMovementArg> Create(T3Time time, float position)
		{
			yield return new UpdateMoveListArg(IsFirst, null, new V1EMoveItem(time, position, Eases.Unmove));
		}

		public IEnumerable<IUpdateMovementArg> Delete()
		{
			return SelectedNodes.Select(node => new UpdateMoveListArg(IsFirst, node.MoveItem, null));
		}

		public IEnumerable<IUpdateMovementArg> ChangeNodeState()
		{
			foreach (var node in SelectedNodes)
			{
				if (node.MoveItem is V1EMoveItem moveItem)
				{
					yield return new UpdateMoveListArg(IsFirst, node.MoveItem, new V1EMoveItem(
						moveItem.Time,
						moveItem.Position,
						TrackMovementEditingManager.Instance.GetNextEase(moveItem.Ease)));
				}
			}
		}

		// System Functions
	}
}