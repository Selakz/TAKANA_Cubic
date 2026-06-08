#nullable enable

using System.ComponentModel;
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
			new PropertyRegistrar<Color>(ISingleton<InScreenEditSetting>.Instance.WidthGridColor, UpdateColor),
			new PropertyRegistrar<Color>(ISingleton<InScreenEditSetting>.Instance.WidthGridSecondColor, UpdateColor)
		};

		// Private
		private GridWidthRetriever widthRetriever = default!;
		private float x;

		// Static

		// Defined Functions
		public void Init(GridWidthRetriever widthRetriever, float x)
		{
			this.widthRetriever = widthRetriever;
			this.x = x;
			transform.localPosition = new Vector3(x, 0, 0);
			widthRetriever.OnBeforeResetGrid += Destroy;
			widthRetriever.EnableSecondColor.PropertyChanged += OnEnableSecondColorChanged;
			UpdateColor();
		}

		private void OnEnableSecondColorChanged(object sender, PropertyChangedEventArgs e) => UpdateColor();

		private void UpdateColor()
		{
			// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
			if (widthRetriever is null) return;
			var setting = ISingleton<InScreenEditSetting>.Instance;
			if (!widthRetriever.EnableSecondColor.Value)
			{
				texture.color = setting.WidthGridColor.Value;
				return;
			}

			int index = Mathf.RoundToInt((x - widthRetriever.GridOffset.Value) / widthRetriever.GridInterval.Value);
			texture.color = index == 0 || Mathf.Abs(index) % 2 == 0
				? setting.WidthGridColor.Value
				: setting.WidthGridSecondColor.Value;
		}

		public void Destroy()
		{
			transform.localPosition = new(0, 100);
			widthRetriever.ReleaseWidthGrid(this);
			widthRetriever.OnBeforeResetGrid -= Destroy;
			widthRetriever.EnableSecondColor.PropertyChanged -= OnEnableSecondColorChanged;
		}

		// System Functions
	}
}