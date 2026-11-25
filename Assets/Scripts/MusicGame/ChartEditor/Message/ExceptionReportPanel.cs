#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.ChartEditor.Message
{
	public class ExceptionReportPanel : HierarchySystem<ExceptionReportPanel>
	{
		// Serializable and Public
		[SerializeField] private GameObject root = default!;
		[SerializeField] private TMP_InputField reporterNameInput = default!;
		[SerializeField] private TMP_InputField summaryInput = default!;
		[SerializeField] private TMP_Dropdown timeRangeDropdown = default!;
		[SerializeField] private I18NTextBlock matchedLogCountText = default!;
		[SerializeField] private Toggle includeProjectToggle = default!;
		[SerializeField] private I18NTextBlock projectHintText = default!;
		[SerializeField] private I18NTextBlock statusText = default!;
		[SerializeField] private Button submitButton = default!;
		[SerializeField] private Button cancelButton = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(submitButton, () => SubmitAsync().Forget()),
			new ButtonRegistrar(cancelButton, Close),
			new DropdownRegistrar(timeRangeDropdown, _ => UpdateMatchedLogCount()),
			new PropertyRegistrar<LevelInfo?>(levelInfo, _ =>
			{
				if (isOpen) UpdateProjectState();
			}),
			new CustomRegistrar(
				() => I18NSystem.OnLanguageChanged += OnLanguageChanged,
				() => I18NSystem.OnLanguageChanged -= OnLanguageChanged)
		};

		// Private
		[Inject] private readonly NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private readonly IExceptionLogService exceptionLogService = default!;
		[Inject] private readonly IExceptionReportService reportService = default!;

		private bool isOpen;
		private bool isSubmitting;
		private ExceptionTimeRange[] timeRanges = Array.Empty<ExceptionTimeRange>();

		// Defined Functions
		public void Open()
		{
			if (isOpen) return;

			RefreshTimeRangeOptions();
			var setting = ISingletonSetting<EditorSetting>.Instance;
			string defaultName = !string.IsNullOrWhiteSpace(setting.LastReporterName.Value)
				? setting.LastReporterName.Value
				: setting.LastCharterId.Value;
			reporterNameInput.SetTextWithoutNotify(defaultName);

			timeRangeDropdown.SetValueWithoutNotify((int)ExceptionTimeRange.Last1Hour);
			UpdateMatchedLogCount();
			UpdateProjectState();

			statusText.gameObject.SetActive(false);
			SetInteractable(true);

			isOpen = true;
			root.SetActive(true);
			ISingleton<InputManager>.Instance.GlobalInputEnabled.Value = false;
		}

		public void Close()
		{
			if (isSubmitting || !isOpen) return;

			isOpen = false;
			root.SetActive(false);
			ISingleton<InputManager>.Instance.GlobalInputEnabled.Value = true;
		}

		private void RefreshTimeRangeOptions()
		{
			int previousValue = timeRangeDropdown.value;
			timeRanges = timeRangeDropdown.SetOptions(
				new List<ExceptionTimeRange>
				{
					ExceptionTimeRange.CurrentSession,
					ExceptionTimeRange.Last15Minutes,
					ExceptionTimeRange.Last1Hour,
					ExceptionTimeRange.Today
				},
				range => I18NSystem.GetText(range switch
				{
					ExceptionTimeRange.CurrentSession => "ExceptionReport_RangeCurrentSession",
					ExceptionTimeRange.Last15Minutes => "ExceptionReport_RangeLast15Minutes",
					ExceptionTimeRange.Last1Hour => "ExceptionReport_RangeLast1Hour",
					ExceptionTimeRange.Today => "ExceptionReport_RangeToday",
					_ => "ExceptionReport_RangeLast1Hour"
				}));
			timeRangeDropdown.SetValueWithoutNotify(Mathf.Clamp(previousValue, 0, timeRanges.Length - 1));
		}

		private void UpdateMatchedLogCount()
		{
			ExceptionTimeRange range = GetSelectedTimeRange();
			int count = exceptionLogService.GetRecords(range).Count;
			matchedLogCountText.SetText("ExceptionReport_MatchedLogCount", count.ToString());
		}

		private void UpdateProjectState()
		{
			if (levelInfo.Value is not { } info)
			{
				includeProjectToggle.SetIsOnWithoutNotify(false);
				includeProjectToggle.interactable = false;
				projectHintText.SetText("ExceptionReport_NoProject");
				return;
			}

			includeProjectToggle.interactable = !isSubmitting;
			if (!isSubmitting)
			{
				includeProjectToggle.SetIsOnWithoutNotify(true);
			}

			string displayName = !string.IsNullOrWhiteSpace(info.SongInfo.Title.Value)
				? info.SongInfo.Title.Value
				: Path.GetFileNameWithoutExtension(info.LevelPath);
			projectHintText.SetText("ExceptionReport_ProjectSelected", displayName);
		}

		private ExceptionTimeRange GetSelectedTimeRange()
		{
			int index = timeRangeDropdown.value;
			return index >= 0 && index < timeRanges.Length
				? timeRanges[index]
				: ExceptionTimeRange.Last1Hour;
		}

		private void SetInteractable(bool interactable)
		{
			reporterNameInput.interactable = interactable;
			summaryInput.interactable = interactable;
			timeRangeDropdown.interactable = interactable;
			includeProjectToggle.interactable = interactable && levelInfo.Value is not null;
			submitButton.interactable = interactable;
			cancelButton.interactable = interactable;
		}

		private async UniTaskVoid SubmitAsync()
		{
			if (isSubmitting) return;

			isSubmitting = true;
			SetInteractable(false);
			statusText.gameObject.SetActive(true);
			statusText.SetText("ExceptionReport_Submitting");

			string reporterName = reporterNameInput.text.Trim();
			var setting = ISingletonSetting<EditorSetting>.Instance;
			if (!string.Equals(setting.LastReporterName.Value, reporterName, StringComparison.Ordinal))
			{
				setting.LastReporterName.Value = reporterName;
				ISingletonSetting<EditorSetting>.SaveInstance();
			}

			ExceptionReportResult result;
			try
			{
				result = await reportService.SubmitReportAsync(
					reporterName,
					summaryInput.text,
					GetSelectedTimeRange(),
					includeProjectToggle.isOn && levelInfo.Value is not null);
			}
			finally
			{
				isSubmitting = false;
				if (this != null) SetInteractable(true);
			}

			if (this == null) return;

			if (result.IsSuccess)
			{
				exceptionLogService.ClearUnreadCount();
				summaryInput.SetTextWithoutNotify(string.Empty);
				statusText.gameObject.SetActive(false);
				Close();

				if (result.ExcludedAudio || result.ExcludedCover)
				{
					T3Logger.Log(
						"Notice",
						$"ExceptionReport_SubmitSuccessTrimmed|{result.MaxArchiveSizeMb}",
						T3LogType.Success);
				}
				else
				{
					T3Logger.Log("Notice", "ExceptionReport_SubmitSuccess", T3LogType.Success);
				}
			}
			else
			{
				statusText.gameObject.SetActive(true);
				statusText.SetText("ExceptionReport_SubmitFailed", result.ErrorMessage ?? "Unknown error");
			}
		}

		private void OnLanguageChanged(LanguagePack _)
		{
			RefreshTimeRangeOptions();
			if (isOpen)
			{
				UpdateMatchedLogCount();
				UpdateProjectState();
			}
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			root.SetActive(false);
			RefreshTimeRangeOptions();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (isOpen)
			{
				ISingleton<InputManager>.Instance.GlobalInputEnabled.Value = true;
			}
		}
	}
}