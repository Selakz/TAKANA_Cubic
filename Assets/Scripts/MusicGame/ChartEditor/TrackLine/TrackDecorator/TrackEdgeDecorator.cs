using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.Message;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.ChartEditor.TrackLine.MoveListDecorator;
using MusicGame.Components.Movement;
using MusicGame.Components.Tracks;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.MVC;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine.TrackDecorator
{
	public class TrackEdgeDecorator : MonoBehaviour, ITrackDecorator
	{
		// Serializable and Public
		[SerializeField] private Transform indicator;
		[SerializeField] private GameObject leftMoveListDecoratorObject;
		[SerializeField] private GameObject rightMoveListDecoratorObject;

		public Track Model { get; private set; }

		public IModel GenericModel => Model;

		public GameObject Object => gameObject;

		public bool IsEditing =>
			leftMoveListDecorator.SelectedNodes.Count() + rightMoveListDecorator.SelectedNodes.Count() > 0;

		public IMoveListDecorator MoveListDecorator1 => leftMoveListDecorator;

		public IMoveListDecorator MoveListDecorator2 => rightMoveListDecorator;

		// Private
		private V1LSMoveList alignerMoveList;
		private IMoveListDecorator leftMoveListDecorator;
		private IMoveListDecorator rightMoveListDecorator;

		// Static

		// Defined Functions
		public void Init(Track track)
		{
			if (Model != null) Model.Movement.OnTrackMovementUpdated -= TrackMovementUpdated;
			Model = track;
			Model.Movement.OnTrackMovementUpdated += TrackMovementUpdated;
			indicator.localPosition = new(Model.Movement.GetPos(Model.TimeInstantiate), indicator.localPosition.y);
			indicator.localScale = new(Model.Movement.GetWidth(Model.TimeInstantiate), 0.15f);
			alignerMoveList = new(track.TimeInstantiate, 0);
			leftMoveListDecorator = IMoveListDecorator.Decorate(leftMoveListDecoratorObject,
				true, Model.TimeInstantiate, Model.Movement.Movement1, LevelManager.Instance.LevelSpeed.SpeedRate);
			rightMoveListDecorator = IMoveListDecorator.Decorate(rightMoveListDecoratorObject,
				false, Model.TimeInstantiate, Model.Movement.Movement2, LevelManager.Instance.LevelSpeed.SpeedRate);
		}

		public void Destroy()
		{
			Destroy(Object);
		}

		public void Disable()
		{
			Uneditable();
			Object.SetActive(false);
		}

		public void Enable()
		{
			Object.SetActive(true);
		}

		public void Uneditable()
		{
			leftMoveListDecorator.IsEditable = false;
			rightMoveListDecorator.IsEditable = false;
		}

		public void Editable()
		{
			// Because making editable may fail, Uneditable() and Editable() are not set to one property.
			if (!Object.activeSelf) return;
			leftMoveListDecorator.IsEditable = true;
			rightMoveListDecorator.IsEditable = true;
		}

		public void UnselectAll()
		{
			leftMoveListDecorator.UnselectAll();
			rightMoveListDecorator.UnselectAll();
		}

		private ICommand To(IEnumerable<IUpdateMovementArg> args)
		{
			if (Model.Movement.Movement1 is IMoveList && Model.Movement.Movement2 is IMoveList)
			{
				var command = new UpdateMoveListCommand(args);
				return command.SetInit(Model) ? command : EmptyCommand.Instance;
			}

			throw new NotImplementedException();
		}

		public ICommand ToLeft()
		{
			return To(leftMoveListDecorator.ToLeft().Concat(rightMoveListDecorator.ToLeft()));
		}

		public ICommand ToRight()
		{
			return To(leftMoveListDecorator.ToRight().Concat(rightMoveListDecorator.ToRight()));
		}

		public ICommand ToLeftGrid()
		{
			return InScreenEditManager.Instance.WidthRetriever is not GridWidthRetriever
				? EmptyCommand.Instance
				: To(leftMoveListDecorator.ToLeftGrid().Concat(rightMoveListDecorator.ToLeftGrid()));
		}

		public ICommand ToRightGrid()
		{
			return InScreenEditManager.Instance.WidthRetriever is not GridWidthRetriever
				? EmptyCommand.Instance
				: To(leftMoveListDecorator.ToRightGrid().Concat(rightMoveListDecorator.ToRightGrid()));
		}

		public ICommand ToNext()
		{
			return To(leftMoveListDecorator.ToNext().Concat(rightMoveListDecorator.ToNext()));
		}

		public ICommand ToPrevious()
		{
			return To(leftMoveListDecorator.ToPrevious().Concat(rightMoveListDecorator.ToPrevious()));
		}

		public ICommand ToNextBeat()
		{
			return InScreenEditManager.Instance.TimeRetriever is not GridTimeRetriever
				? EmptyCommand.Instance
				: To(leftMoveListDecorator.ToNextBeat().Concat(rightMoveListDecorator.ToNextBeat()));
		}

		public ICommand ToPreviousBeat()
		{
			return InScreenEditManager.Instance.TimeRetriever is not GridTimeRetriever
				? EmptyCommand.Instance
				: To(leftMoveListDecorator.ToPreviousBeat().Concat(rightMoveListDecorator.ToPreviousBeat()));
		}

		public ICommand Create(Vector2 gamePoint)
		{
			float baseTime = InScreenEditManager.Instance.TimeRetriever.GetTimeStart(gamePoint);
			float basePosition = gamePoint.x;
			float actualPosition = InScreenEditManager.Instance.WidthRetriever.GetAttachedPosition(gamePoint);
			if (baseTime < Model.TimeInstantiate || baseTime > Model.TimeEnd)
			{
				HeaderMessage.Show("添加失败：目标时间在轨道时间范围外", HeaderMessage.MessageType.Warn);
				return EmptyCommand.Instance;
			}

			float leftX = Model.Movement.GetLeftPos(baseTime), rightX = Model.Movement.GetRightPos(baseTime);
			bool isLeft = basePosition < (leftX + rightX) / 2;
			var moveListDecorator = isLeft ? leftMoveListDecorator : rightMoveListDecorator;
			return To(moveListDecorator.Create(baseTime, actualPosition));
		}

		public ICommand Delete()
		{
			return To(leftMoveListDecorator.Delete().Concat(rightMoveListDecorator.Delete()));
		}

		public ICommand ChangeNodeState()
		{
			return To(leftMoveListDecorator.ChangeNodeState().Concat(rightMoveListDecorator.ChangeNodeState()));
		}

		// Event Handlers
		private void TrackMovementUpdated()
		{
			alignerMoveList.BaseTime = Model.TimeInstantiate;
			indicator.localPosition = new(Model.Movement.GetPos(Model.TimeInstantiate), indicator.localPosition.y);
			indicator.localScale = new(Model.Movement.GetWidth(Model.TimeInstantiate), 0.15f);
			leftMoveListDecorator.Init
				(true, Model.TimeInstantiate, Model.Movement.Movement1, LevelManager.Instance.LevelSpeed.SpeedRate);
			rightMoveListDecorator.Init
				(false, Model.TimeInstantiate, Model.Movement.Movement2, LevelManager.Instance.LevelSpeed.SpeedRate);
		}

		// System Functions
		void Update()
		{
			var y = -alignerMoveList.GetPos(LevelManager.Instance.Music.ChartTime) *
			        LevelManager.Instance.LevelSpeed.SpeedRate;
			Object.transform.localPosition = new(0, y);
		}

		void OnEnable()
		{
			leftMoveListDecorator?.Init
				(true, Model.TimeInstantiate, Model.Movement.Movement1, LevelManager.Instance.LevelSpeed.SpeedRate);
			rightMoveListDecorator?.Init
				(false, Model.TimeInstantiate, Model.Movement.Movement2, LevelManager.Instance.LevelSpeed.SpeedRate);
		}
	}
}