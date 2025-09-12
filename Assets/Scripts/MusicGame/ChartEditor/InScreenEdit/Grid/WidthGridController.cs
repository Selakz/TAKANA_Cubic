using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	public class WidthGridController : MonoBehaviour
	{
		// Serializable and Public

		// Private
		private GridWidthRetriever widthRetriever;

		// Static

		// Defined Functions
		public void Init(GridWidthRetriever widthRetriever, float x)
		{
			this.widthRetriever = widthRetriever;
			transform.localPosition = new Vector3(x, 0, 0);
			widthRetriever.OnBeforeResetGrid += Destroy;
		}

		public void Destroy()
		{
			transform.localPosition = new(0, 100);
			widthRetriever.ReleaseWidthGrid(this);
			widthRetriever.OnBeforeResetGrid -= Destroy;
		}

		// System Functions
	}
}