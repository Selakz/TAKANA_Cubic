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
		[SerializeField] private Highlight2D highlight;
		[SerializeField] private LineRenderer lineRenderer;
		[SerializeField] private BoxCollider2D boxCollider;
		[SerializeField] private EdgeCollider2D edgeCollider;

		public bool IsSelected
		{
			get => highlight.IsHighlight;
			set
			{
				highlight.IsHighlight = value;
				lineRenderer.material.color = value ? new Color(0.38f, 0.60f, 0.82f) : Color.white;
			}
		}

		public IMoveItem MoveItem { get; private set; }

		public bool IsEditable
		{
			get => isEditable;
			set
			{
				if (isEditable == value) return;
				isEditable = value;
				boxCollider.gameObject.SetActive(value);
				lineRenderer.startWidth = lineRenderer.endWidth =
					value
						? ISingletonSetting<TrackLineSetting>.Instance.EditableLineWidth
						: ISingletonSetting<TrackLineSetting>.Instance.UneditableLineWidth;
				edgeCollider.enabled = value;
			}
		}

		// Private
		private Vector2 currentBuffer;
		private Vector2 nextBuffer;
		private Eases easeBuffer;

		private bool isEditable;

		// Defined Functions
		public void Init(V1EMoveItem moveItem, Vector2 current, Vector2 next, Eases easeType)
		{
			MoveItem = moveItem;
			if (current == currentBuffer && next == nextBuffer && easeBuffer == easeType)
			{
				return;
			}

			currentBuffer = current;
			nextBuffer = next;
			easeBuffer = easeType;
			transform.localPosition = current;
			LineDrawer.DrawCurve(lineRenderer, new(), next - current, easeType.Opposite().GetString());
			LineDrawer.DrawCurve(edgeCollider, new(), next - current, easeType.Opposite().GetString());
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
			if (InScreenEditManager.Instance.WidthRetriever is not GridWidthRetriever widthRetriever) return MoveItem;
			V1EMoveItem moveItem = (MoveItem as V1EMoveItem?)!.Value;
			var newPosition = widthRetriever.GetLeftAttachedPosition(moveItem.Position);
			return new V1EMoveItem(moveItem.Time, newPosition, moveItem.Ease);
		}

		public IMoveItem ToRightGrid()
		{
			if (InScreenEditManager.Instance.WidthRetriever is not GridWidthRetriever widthRetriever) return MoveItem;
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
			if (InScreenEditManager.Instance.TimeRetriever is not GridTimeRetriever timeRetriever) return MoveItem;
			V1EMoveItem moveItem = (MoveItem as V1EMoveItem?)!.Value;
			var newTime = timeRetriever.GetCeilTime(moveItem.Time);
			return moveItem.SetTime(newTime);
		}

		public IMoveItem ToPreviousBeat()
		{
			if (InScreenEditManager.Instance.TimeRetriever is not GridTimeRetriever timeRetriever) return MoveItem;
			V1EMoveItem moveItem = (MoveItem as V1EMoveItem?)!.Value;
			var newTime = timeRetriever.GetFloorTime(moveItem.Time);
			return moveItem.SetTime(newTime);
		}

		// System Functions
	}
}