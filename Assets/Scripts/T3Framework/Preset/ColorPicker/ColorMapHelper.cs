#nullable enable

using UnityEngine;

namespace T3Framework.Preset.ColorPicker
{
	public static class ColorMapHelper
	{
		public static Texture2D GetColorMapTexture(int width, int height, float hue)
		{
			var texture = new Texture2D(width, height);
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					Color color = GetColorOfPosition(width, height, x, y, hue);
					texture.SetPixel(x, y, color);
				}
			}

			texture.Apply();
			return texture;
		}

		public static Texture2D GetHueSliderTexture(int width, int height)
		{
			var texture = new Texture2D(width, height);
			for (int x = 0; x < width; x++)
			{
				float hue = (float)x / width;
				Color color = Color.HSVToRGB(hue, 1f, 1f);
				for (int y = 0; y < height; y++)
				{
					texture.SetPixel(x, y, color);
				}
			}

			texture.Apply();
			return texture;
		}

		public static Color GetColorOfPosition(int width, int height, int x, int y, float hue)
		{
			float saturation = x / (float)width;
			float value = y / (float)height;

			return Color.HSVToRGB(hue, saturation, value);
		}

		/// <returns> Hue of the color. </returns>
		public static float GetPositionOfColor(int width, int height, Color color, out int x, out int y)
		{
			Color.RGBToHSV(color, out float h, out float s, out float v);
			x = Mathf.RoundToInt(s * width);
			y = Mathf.RoundToInt(v * height);
			return h;
		}
	}
}