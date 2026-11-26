#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	public class TimeGridController : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private SpriteRenderer lineImage = default!;
		[SerializeField] private TimingBlock timingBlock = default!;
		[SerializeField] private RectTransform timingBlockTransform = default!;

		public TimingBlock TimingBlock => timingBlock;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(timingBlock.IsHovered,
				isHovered => transform.localScale = new(1, isHovered ? 1.5f : 1f, 1))
		};

		// Defined Functions
		public void SetTimingBlockParent(Transform parent) => timingBlockTransform.SetParent(parent, false);

		public void SetData(T3Time time, Color color)
		{
			lineImage.color = color;
			timingBlock.Init(time, color);
		}

		public void SetPositionY(float y, float? timingBlockY)
		{
			transform.localPosition = new(0, y);
			if (timingBlockY.HasValue)
			{
				timingBlock.gameObject.SetActive(true);
				timingBlockTransform.anchoredPosition = new(0, timingBlockY.Value);
			}
			else
			{
				timingBlock.gameObject.SetActive(false);
			}
		}

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			timingBlock.gameObject.SetActive(true);
		}

		protected override void OnDisable()
		{
			timingBlock.gameObject.SetActive(false);
			base.OnDisable();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (timingBlock != null) Destroy(timingBlock.gameObject);
		}
	}
}