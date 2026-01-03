#nullable enable

using System.Collections.Generic;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLine.UI
{
	public class EaseSwitcher : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private GameObject headerCurveTextObject = default!;
		[SerializeField] private TMP_Text currentCurveText = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("CurveSwitch", "SwitchToSine", () => ChangeEase(1)),
			new InputRegistrar("CurveSwitch", "SwitchToQuad", () => ChangeEase(2)),
			new InputRegistrar("CurveSwitch", "SwitchToCubic", () => ChangeEase(3)),
			new InputRegistrar("CurveSwitch", "SwitchToQuart", () => ChangeEase(4)),
			new InputRegistrar("CurveSwitch", "SwitchToQuint", () => ChangeEase(5)),
			new InputRegistrar("CurveSwitch", "SwitchToExpo", () => ChangeEase(6)),
			new InputRegistrar("CurveSwitch", "SwitchToCirc", () => ChangeEase(7)),
			new InputRegistrar("CurveSwitch", "SwitchToBack", () => ChangeEase(8)),
			new InputRegistrar("CurveSwitch", "SwitchToElastic", () => ChangeEase(9)),
			new InputRegistrar("CurveSwitch", "SwitchToBounce", () => ChangeEase(0)),
			new InputRegistrar("CurveSwitch", "CheckCurve", CheckEase, InputActionPhase.Started),
			new InputRegistrar("CurveSwitch", "CheckCurve", HideEase, InputActionPhase.Performed),
			new InputRegistrar("CurveSwitch", "CheckCurve", HideEase, InputActionPhase.Canceled),
		};

		// Private
		private readonly Dictionary<int, string> easeIdToName = new()
		{
			{ 1, "Sine" },
			{ 2, "Quad" },
			{ 3, "Cubic" },
			{ 4, "Quart" },
			{ 5, "Quint" },
			{ 6, "Expo" },
			{ 7, "Circ" },
			{ 8, "Back" },
			{ 9, "Elastic" },
			{ 0, "Bounce" },
		};

		private NotifiableProperty<int> easeId = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			[Key("ease-id")] NotifiableProperty<int> easeId)
		{
			this.easeId = easeId;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Handlers
		private void ChangeEase(int id)
		{
			easeId.Value = id;
			T3Logger.Log("Notice", $"TrackLine_ChangeEase|{easeIdToName[id]}", T3LogType.Info);
		}

		private void CheckEase()
		{
			currentCurveText.text = I18NSystem.GetText("TrackLine_CheckEase", easeIdToName[easeId]);
			headerCurveTextObject.SetActive(true);
		}

		private void HideEase()
		{
			headerCurveTextObject.SetActive(false);
		}
	}
}