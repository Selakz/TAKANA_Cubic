using System.Collections.Generic;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Components.Movement;
using T3Framework.Runtime;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace MusicGame.ChartEditor.TrackLine.MoveListDecorator
{
	public interface IMoveListDecorator
	{
		public bool IsEditable { get; set; }

		public IMovement<float> Movement { get; }

		public IEnumerable<IMovementNode> SelectedNodes { get; }

		public static IMoveListDecorator Decorate
			(GameObject attachedObject, bool isFirst, T3Time timeTrackStart, IMovement<float> movement, float speedRate)
		{
			switch (movement)
			{
				case V1EMoveList v1e:
					var decorator = attachedObject.AddComponent<V1EDecorator>();
					decorator.Init(isFirst, timeTrackStart, v1e, speedRate);
					return decorator;
				default:
					// TODO: return fallbackDecorator
					return null;
			}
		}

		public void Init(bool isFirst, T3Time timeTrackStart, IMovement<float> movement, float speedRate);

		/// <summary> Used when only the move list content is updated. Otherwise, use <see cref="Init"/> again. </summary>
		public void Refresh();

		public void UnselectAll();

		public void Destroy();

		IEnumerable<IUpdateMovementArg> ToLeft();

		IEnumerable<IUpdateMovementArg> ToRight();

		IEnumerable<IUpdateMovementArg> ToLeftGrid();

		IEnumerable<IUpdateMovementArg> ToRightGrid();

		IEnumerable<IUpdateMovementArg> ToNext();

		IEnumerable<IUpdateMovementArg> ToPrevious();

		IEnumerable<IUpdateMovementArg> ToNextBeat();

		IEnumerable<IUpdateMovementArg> ToPreviousBeat();

		IEnumerable<IUpdateMovementArg> Create(T3Time time, float position);

		IEnumerable<IUpdateMovementArg> Delete();

		IEnumerable<IUpdateMovementArg> ChangeNodeState();
	}
}