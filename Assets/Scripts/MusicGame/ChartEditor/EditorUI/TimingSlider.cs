using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TimingSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, ICanEnableUI
{
	// Serializable and Public
	public static TimingSlider Instance { get; private set; }
	public Slider slider;

	public void Enable()
	{
		slider.interactable = true;
	}

	// Private
	private bool isPointerDown;

	// Static

	// Defined Function

	// System Function
	void Awake()
	{
		Instance = this;
	}

	void Update()
	{
		if (slider.interactable && !isPointerDown)
			slider.value = TimeProvider.Instance.AudioTime / TimeProvider.Instance.AudioLength;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		isPointerDown = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		isPointerDown = false;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (slider.interactable)
		{
			EditingLevelManager.Instance.AskForResetFieldTo(TimeProvider.Instance.AudioLength * slider.value);
		}
	}
}
