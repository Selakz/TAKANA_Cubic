#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Static;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	public class WidthGridController : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private SpriteRenderer texture = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<Color>(ISingleton<InScreenEditSetting>.Instance.WidthGridColor,
				color => { texture.color = color; })
		};

		// Private
		private GridWidthRetriever widthRetriever = default!;

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