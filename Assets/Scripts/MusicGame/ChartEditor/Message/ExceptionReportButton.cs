#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.ChartEditor.Message
{
	public class ExceptionReportButton : HierarchySystem<ExceptionReportButton>
	{
		// Serializable and Public
		[SerializeField] private GameObject root = default!;
		[SerializeField] private Button openReportButton = default!;
		[SerializeField] private Button dismissButton = default!;
		[Header("Optional")] [SerializeField] private TMP_Text? countText;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<int>(exceptionLogService.UnreadExceptionCount, UpdateUnreadState),
			new ButtonRegistrar(openReportButton, () => reportPanel.Open()),
			new ButtonRegistrar(dismissButton, () => exceptionLogService.ClearUnreadCount())
		};

		// Private
		[Inject] private readonly IExceptionLogService exceptionLogService = default!;
		[Inject] private readonly ExceptionReportPanel reportPanel = default!;

		// Defined Functions
		private void UpdateUnreadState(int count)
		{
			root.SetActive(count > 0);
			if (countText is not null)
			{
				countText.text = count > 99 ? "99+" : count.ToString();
			}
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			root.SetActive(false);
		}
	}
}
