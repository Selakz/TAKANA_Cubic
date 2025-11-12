#nullable enable

using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.Components.Movement;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.UI;
using T3Framework.Static.Easing;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine.MoveListDecorator
{
	// TODO: Implement IModifiableView2D
	public class V1ENode : MonoBehaviour, IMovementNode
	{
		// Serializable and Public
		[SerializeField] private Highlight2D highlight = default!;
		[SerializeField] private MeshFilter meshFilter = default!;
		[SerializeField] private MeshRenderer meshRenderer = default!;
		[SerializeField] private BoxCollider boxCollider = default!;
		[SerializeField] private MeshCollider meshCollider = default!;

		public bool IsSelected
		{
			get => highlight.IsHighlight;
			set
			{
				highlight.IsHighlight = value;
				meshRenderer.material.color = value ? new Color(0.38f, 0.60f, 0.82f) : Color.white;
			}
		}

		public IMoveItem? MoveItem { get; private set; }

		public bool IsEditable
		{
			get => isEditable;
			set
			{
				if (isEditable == value) return;
				isEditable = value;
				boxCollider.gameObject.SetActive(value);
				meshCollider.enabled = value;

				if (viewMesh is null) return;
				var setting = ISingletonSetting<TrackLineSetting>.Instance;
				var maxSegment = setting.MaxSegment.Value;
				var viewLineWidth =
					isEditable ? setting.EditableLineWidth.Value : setting.UneditableLineWidth.Value;
				var viewLinePrecision = setting.ViewLinePrecision.Value;
				LineDrawer.DrawMesh(viewMesh, easeBuffer.Opposite(), viewLineWidth,
					nextBuffer.x - currentBuffer.x, nextBuffer.y - currentBuffer.y,
					viewLinePrecision, maxSegment);
			}
		}

		// Private
		private Vector2 currentBuffer;
		private Vector2 nextBuffer;
		private Eases easeBuffer;
		private Mesh? viewMesh;
		private Mesh? logicMesh;

		private bool isEditable;

		// Defined Functions
		public void Init(V1EMoveItem moveItem, Vector2 current, Vector2 next)
		{
			MoveItem = moveItem;
			if (current == currentBuffer && next == nextBuffer && easeBuffer == moveItem.Ease) return;

			currentBuffer = current;
			nextBuffer = next;
			easeBuffer = moveItem.Ease;
			transform.localPosition = new(current.x, current.y, -0.01f);
			if (current != next)
			{
				viewMesh ??= new();
				var setting = ISingletonSetting<TrackLineSetting>.Instance;
				var maxSegment = setting.MaxSegment.Value;
				var viewLineWidth =
					IsEditable ? setting.EditableLineWidth.Value : setting.UneditableLineWidth.Value;
				var viewLinePrecision = setting.ViewLinePrecision.Value;
				LineDrawer.DrawMesh(viewMesh, moveItem.Ease.Opposite(), viewLineWidth,
					next.x - current.x, next.y - current.y,
					viewLinePrecision, maxSegment);
				meshFilter.mesh = viewMesh;
				logicMesh ??= new();
				var logicLineWidth = setting.LogicLineWidth.Value;
				var logicLinePrecision = setting.LogicLinePrecision.Value;
				LineDrawer.DrawMesh(logicMesh, moveItem.Ease.Opposite(), logicLineWidth,
					next.x - current.x, next.y - current.y,
					logicLinePrecision, maxSegment);
				meshCollider.sharedMesh = logicMesh;
			}
		}

		public IMoveItem ToLeft()
		{
			V1EMoveItem moveItem = (MoveItem as V1EMoveItem?)!.Value;
			return new V1EMoveItem
			(moveItem.Time,
				moveItem.Position - ISingletonSetting<TrackLineSetting>.Instance.NodePositionNudgeDistance,
				moveItem.Ease);
		}

		public IMoveItem ToRight()
		{
			V1EMoveItem moveItem = (MoveItem as V1EMoveItem?)!.Value;
			return new V1EMoveItem
			(moveItem.Time,
				moveItem.Position + ISingletonSetting<TrackLineSetting>.Instance.NodePositionNudgeDistance,
				moveItem.Ease);
		}

		public IMoveItem ToLeftGrid()
		{
			if (InScreenEditManager.Instance.WidthRetriever is not GridWidthRetriever widthRetriever) return MoveItem!;
			V1EMoveItem moveItem = (MoveItem as V1EMoveItem?)!.Value;
			var newPosition = widthRetriever.GetLeftAttachedPosition(moveItem.Position);
			return new V1EMoveItem(moveItem.Time, newPosition, moveItem.Ease);
		}

		public IMoveItem ToRightGrid()
		{
			if (InScreenEditManager.Instance.WidthRetriever is not GridWidthRetriever widthRetriever) return MoveItem!;
			V1EMoveItem moveItem = (MoveItem as V1EMoveItem?)!.Value;
			var newPosition = widthRetriever.GetRightAttachedPosition(moveItem.Position);
			return new V1EMoveItem(moveItem.Time, newPosition, moveItem.Ease);
		}

		public IMoveItem ToNext()
		{
			V1EMoveItem moveItem = (MoveItem as V1EMoveItem?)!.Value;
			return moveItem.SetTime(moveItem.Time + ISingletonSetting<TrackLineSetting>.Instance.NodeTimeNudgeDistance);
		}

		public IMoveItem ToPrevious()
		{
			V1EMoveItem moveItem = (MoveItem as V1EMoveItem?)!.Value;
			return moveItem.SetTime(moveItem.Time - ISingletonSetting<TrackLineSetting>.Instance.NodeTimeNudgeDistance);
		}

		public IMoveItem ToNextBeat()
		{
			if (InScreenEditManager.Instance.TimeRetriever is not GridTimeRetriever timeRetriever) return MoveItem!;
			V1EMoveItem moveItem = (MoveItem as V1EMoveItem?)!.Value;
			var newTime = timeRetriever.GetCeilTime(moveItem.Time);
			return moveItem.SetTime(newTime);
		}

		public IMoveItem ToPreviousBeat()
		{
			if (InScreenEditManager.Instance.TimeRetriever is not GridTimeRetriever timeRetriever) return MoveItem!;
			V1EMoveItem moveItem = (MoveItem as V1EMoveItem?)!.Value;
			var newTime = timeRetriever.GetFloorTime(moveItem.Time);
			return moveItem.SetTime(newTime);
		}

		// System Functions
	}
}