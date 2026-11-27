#nullable enable

using System.Globalization;
using MusicGame.ChartEditor.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace EditorPlugin.RemoteBridge.UI
{
	public class RemoteBridgeSettingUI : HierarchySystem<RemoteBridgeSettingUI>
	{
		// Serializable and Public
		[SerializeField] private Toggle enabledToggle = default!;
		[SerializeField] private TMP_InputField portInputField = default!;
		[SerializeField] private Image? statusIndicator;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ToggleRegistrar(enabledToggle, OnEnabledToggleChanged),
			new InputFieldRegistrar(portInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, OnPortEndEdit),
			new PropertyRegistrar<bool>(ISingletonSetting<EditorSetting>.Instance.RemoteBridgeEnabled,
				enabled => enabledToggle.SetIsOnWithoutNotify(enabled)),
			new PropertyRegistrar<int>(ISingletonSetting<EditorSetting>.Instance.RemoteBridgePort,
				port => portInputField.SetTextWithoutNotify(port.ToString(CultureInfo.InvariantCulture))),
			new PropertyRegistrar<RemoteBridgeStatus>(remoteBridgeService.Status, UpdateStatusIndicator)
		};

		// Private
		[Inject] private IRemoteBridgeService remoteBridgeService = default!;

		private const int MinPort = 1024;
		private const int MaxPort = 65535;

		// Static
		private static readonly Color stoppedColor = new(0.6f, 0.6f, 0.6f, 1f);
		private static readonly Color listeningColor = new(0.2f, 0.78f, 0.35f, 1f);
		private static readonly Color errorColor = new(0.9f, 0.25f, 0.25f, 1f);

		// Defined Functions
		private void OnEnabledToggleChanged(bool enabled)
		{
			var setting = ISingletonSetting<EditorSetting>.Instance;
			setting.RemoteBridgeEnabled.Value = enabled;
			ISingletonSetting<EditorSetting>.SaveInstance();
		}

		private void OnPortEndEdit(string text)
		{
			var setting = ISingletonSetting<EditorSetting>.Instance;
			if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int port))
			{
				port = Mathf.Clamp(port, MinPort, MaxPort);
				setting.RemoteBridgePort.Value = port;
				setting.RemoteBridgePort.AddUpNotify();
				ISingletonSetting<EditorSetting>.SaveInstance();
			}

			portInputField.SetTextWithoutNotify(
				setting.RemoteBridgePort.Value.ToString(CultureInfo.InvariantCulture));
		}

		private void UpdateStatusIndicator(RemoteBridgeStatus status)
		{
			if (statusIndicator is null) return;
			statusIndicator.color = status.State switch
			{
				RemoteBridgeState.Listening => listeningColor,
				RemoteBridgeState.Error => errorColor,
				_ => stoppedColor
			};
		}
	}
}
