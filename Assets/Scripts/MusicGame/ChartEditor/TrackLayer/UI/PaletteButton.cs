using System;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.TrackLayer.UI
{
	[RequireComponent(typeof(Button))]
	public class PaletteButton : MonoBehaviour
	{
		[SerializeField] private Color paletteColor = Color.black;

		public event EventHandler<Color> OnColorClicked;

		public Color PaletteColor
		{
			get => paletteColor;
			set
			{
				paletteColor = value;
				if (button == null) button = GetComponent<Button>();
				button.targetGraphic.color = paletteColor;
			}
		}

		private Button button;

		void Awake()
		{
			button = GetComponent<Button>();
			button.onClick.AddListener(() => OnColorClicked?.Invoke(this, PaletteColor));
		}

		void OnValidate()
		{
			button = GetComponent<Button>();
			button.targetGraphic.color = PaletteColor;
		}
	}
}