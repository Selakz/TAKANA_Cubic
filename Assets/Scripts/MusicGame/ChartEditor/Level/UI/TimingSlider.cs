using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Level.UI
{
	public class TimingSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		// Serializable and Public
		[SerializeField] private Slider slider;

		// Private
		private T3Time SliderTime
		{
			get => (int)slider.value;
			set => slider.value = (int)value;
		}

		private bool isPointerDown;

		// Static

		// Defined Function

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			slider.interactable = true;
			slider.minValue = 0;
			slider.maxValue = Mathf.Floor(levelInfo.Music.length * 1000) - 1;
			slider.wholeNumbers = true;
		}

		// System Function
		void OnEnable()
		{
			EventManager.Instance.AddListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}

		void Update()
		{
			if (slider.interactable && !isPointerDown)
			{
				SliderTime = (int)LevelManager.Instance.Music.AudioTime;
			}
		}

		public void OnPointerDown(PointerEventData eventData) => isPointerDown = true;

		public void OnPointerUp(PointerEventData eventData) => isPointerDown = false;

		public void OnDrag(PointerEventData eventData)
		{
			if (slider.interactable)
			{
				EventManager.Instance.Invoke("Level_OnReset", SliderTime);
			}
		}
	}
}