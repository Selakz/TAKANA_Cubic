#nullable enable

using System.ComponentModel;
using T3Framework.Preset.DataContainers;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace T3Framework.Preset.ColorPicker
{
	public class HsvColorPicker : T3MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
	{
		// Serializable and Public
		[SerializeField] private ColorDataContainer colorDataContainer = default!;
		[SerializeField] private RawImage rawImage = default!;
		[SerializeField] private Slider hueSlider = default!;
		[SerializeField] private RawImage sliderBackground = default!;
		[SerializeField] private Graphic positionIndicator = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<Color, ColorDataContainer>(colorDataContainer, OnColorChanged),
			new SliderRegistrar(hueSlider, OnSliderValueChanged)
		};

		// Private
		private int Width => Mathf.FloorToInt(rawImage.rectTransform.rect.width);
		private int Height => Mathf.FloorToInt(rawImage.rectTransform.rect.height);

		private float hue;

		// Defined Functions
		private void UpdatePosition(Vector2 localPoint)
		{
			var x = Mathf.Clamp(localPoint.x, 0, Width);
			var y = Mathf.Clamp(localPoint.y, 0, Height);
			positionIndicator.transform.localPosition = new(x, y);
			positionIndicator.color = colorDataContainer.Property.Value.Inverse();
		}

		private void SetColor(int x, int y)
		{
			x = Mathf.Clamp(x, 0, Width);
			y = Mathf.Clamp(y, 0, Height);
			var color = ColorMapHelper.GetColorOfPosition(Width, Height, x, y, hue);
			color.a = colorDataContainer.Property.Value.a;
			colorDataContainer.Property.Value = color;
		}

		private void SetColor(float hue)
		{
			var color = colorDataContainer.Property.Value;
			Color.RGBToHSV(color, out float _, out float s, out float v);
			color = Color.HSVToRGB(hue, s, v);
			color.a = colorDataContainer.Property.Value.a;
			colorDataContainer.Property.Value = color;
		}

		// Event Handlers
		private void OnColorChanged(object sender, PropertyChangedEventArgs e)
		{
			var color = colorDataContainer.Property.Value;
			var targetHue = ColorMapHelper.GetPositionOfColor(Width, Height, color, out int x, out int y);
			Color.RGBToHSV(color, out float _, out float s, out float _);
			if (!Mathf.Approximately(s, 0)) hue = targetHue;

			UpdatePosition(new Vector2(x, y));
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (eventData.pointerCurrentRaycast.gameObject != rawImage.gameObject) return;
			var rect = (RectTransform)rawImage.transform;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				rect, Input.mousePosition, null, out var localPoint);
			SetColor((int)localPoint.x, (int)localPoint.y);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (eventData.pointerCurrentRaycast.gameObject != rawImage.gameObject) return;
			Updater.OnUpdate += OnUpdate;
		}

		// Need this to invoke OnBeginDrag and OnEndDrag
		public void OnDrag(PointerEventData eventData)
		{
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			Updater.OnUpdate -= OnUpdate;
		}

		private void OnSliderValueChanged(float value)
		{
			hue = hueSlider.value / hueSlider.maxValue;
			var colorTexture = ColorMapHelper.GetColorMapTexture(Width, Height, hue);
			rawImage.texture = colorTexture;
			SetColor(hue);
		}

		private void OnUpdate()
		{
			var rect = (RectTransform)rawImage.transform;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				rect, Input.mousePosition, null, out var localPoint);
			SetColor((int)localPoint.x, (int)localPoint.y);
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			if (rawImage.TryGetComponent<RectTransform>(out var rectTransform) && rectTransform.pivot != Vector2.zero)
			{
				Debug.LogWarning($"The pivot of the RawImage for color picker should be {Vector2.zero}");
			}

			var rect = sliderBackground.rectTransform.rect;
			sliderBackground.texture = ColorMapHelper.GetHueSliderTexture((int)rect.width, (int)rect.height);
		}
	}
}