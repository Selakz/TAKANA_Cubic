#if UNITY_STANDALONE_WIN
using System.Windows.Forms;
#endif
using T3Framework.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	public class TimingBlock : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
		[SerializeField] private Image image;

		private T3Time time;

		public void Init(T3Time time, Color color)
		{
			this.time = time;
			image.color = color;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
#if UNITY_STANDALONE_WIN
			Clipboard.SetText(time.ToString());
#endif
			if (TimingInputField.Current != null)
			{
				TimingInputField.Current.text = time.ToString();
				TimingInputField.Current.onEndEdit.Invoke(time.ToString());
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			transform.localScale = new(1, 1.5f);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			transform.localScale = new(1, 1);
		}
	}
}