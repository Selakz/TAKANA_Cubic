#nullable enable

using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.Draft
{
	public interface IDraftNoteService
	{
		public float GetMouseAttachedPosition();

		public float GetLeftAttachedPosition(float position);

		public float GetRightAttachedPosition(float position);

		public float GetWidenedGridWidth(float position, float width);

		public float GetNarrowedGridWidth(float position, float width);
	}

	public class BasicDraftNoteService : HierarchySystem<BasicDraftNoteService>, IDraftNoteService
	{
		// Serializable and Public
		public override bool AsImplementedInterfaces => true;

		// Private
		[Inject] private StageMouseWidthRetriever widthRetriever = default!;

		// Defined Functions
		public float GetMouseAttachedPosition()
		{
			return widthRetriever.GetMouseAttachedPosition(out var position) ? position : 0;
		}

		public float GetLeftAttachedPosition(float position)
		{
			return widthRetriever.WidthRetriever.Value is not GridWidthRetriever retriever
				? position
				: retriever.GetLeftAttachedPosition(position);
		}

		public float GetRightAttachedPosition(float position)
		{
			return widthRetriever.WidthRetriever.Value is not GridWidthRetriever retriever
				? position
				: retriever.GetRightAttachedPosition(position);
		}

		public float GetWidenedGridWidth(float position, float width)
		{
			var left = position - width / 2;
			var right = position + width / 2;
			var leftDistance = Mathf.Abs(GetLeftAttachedPosition(left) - left);
			var rightDistance = Mathf.Abs(GetRightAttachedPosition(right) - right);
			return width + 2 * Mathf.Min(leftDistance, rightDistance);
		}

		public float GetNarrowedGridWidth(float position, float width)
		{
			var left = position - width / 2;
			var right = position + width / 2;
			var leftDistance = Mathf.Abs(GetRightAttachedPosition(left) - left);
			var rightDistance = Mathf.Abs(GetLeftAttachedPosition(right) - right);
			var result = width - 2 * Mathf.Min(leftDistance, rightDistance);
			return result < 0.001f ? width : result;
		}
	}
}