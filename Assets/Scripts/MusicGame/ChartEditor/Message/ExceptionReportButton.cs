#nullable enable

using System.Collections.Generic;
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
		[SerializeField] private string? key = null;
		[SerializeField] private Button openReportButton = default!;
		[Header("Optional"), SerializeField] private GameObject? root;
		[SerializeField] private Button dismissButton = default!;
		[SerializeField] private TMP_Text? countText;

		public override string? Key => key;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars
		{
			get
			{
				var list = new List<IEventRegistrar>
				{
					new PropertyRegistrar<int>(exceptionLogService.UnreadExceptionCount, UpdateUnreadState),
					new ButtonRegistrar(openReportButton, () => reportPanel.Open()),
				};
				if (dismissButton != null)
					list.Add(
						new ButtonRegistrar(dismissButton, () => exceptionLogService.ClearUnreadCount()));
				return list.ToArray();
			}
		}

		// Private
		[Inject] private readonly IExceptionLogService exceptionLogService = default!;
		[Inject] private readonly ExceptionReportPanel reportPanel = default!;

		// Defined Functions
		private void UpdateUnreadState(int count)
		{
			if (root != null) root.SetActive(count > 0);
			if (countText != null)
			{
				countText.text = count > 99 ? "99+" : count.ToString();
			}
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			if (root != null) root.SetActive(false);
		}
	}
}