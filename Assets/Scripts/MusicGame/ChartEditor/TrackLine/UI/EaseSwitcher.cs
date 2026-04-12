#nullable enable

using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Movement;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.Threading;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine.UI
{
	public class EaseSwitcher : HierarchySystem<EaseSwitcher>
	{
		// Serializable and Public
		[SerializeField] private InspectorDictionary<int, Toggle> easeToggles = default!;
		[SerializeField] private int closeDelayMs = 1500;
		[SerializeField] private RectTransform panelRoot = default!;
		[SerializeField] private FloatMovementContainer openMovement = default!;
		[SerializeField] private FloatMovementContainer closeMovement = default!;

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
			new InputRegistrar("CurveSwitch", "CheckCurve", CheckEase),

			new PropertyRegistrar<int>(easeId, id =>
			{
				easeToggles.Value[id].SetIsOnWithoutNotify(true);
				if (EventSystem.current != null &&
				    easeToggles.Value.Values.Any(t => t.gameObject == EventSystem.current.currentSelectedGameObject))
				{
					EventSystem.current.SetSelectedGameObject(easeToggles.Value[id].gameObject);
				}
			}),
			new UnionRegistrar(() =>
			{
				List<IEventRegistrar> registrars = new(easeToggles.Value.Count);
				foreach (var (id, toggle) in easeToggles.Value)
				{
					registrars.Add(new ToggleRegistrar(toggle, isOn =>
					{
						if (isOn)
						{
							closeRcts.CancelAndReset();
							ChangeEase(id);
						}
					}));
				}

				return registrars;
			})
		};

		// Private
		private readonly ReusableCancellationTokenSource closeRcts = new();

		private NotifiableProperty<int> easeId = default!;
		private bool isOpening = false;

		// Defined Functions
		[Inject]
		private void Construct(
			[Key("ease-id")] NotifiableProperty<int> easeId)
		{
			this.easeId = easeId;
		}

		// Event Handlers
		private void ChangeEase(int id)
		{
			easeId.Value = id;
			Open();
			WaitForClose();
		}

		private void CheckEase()
		{
			Open();
			WaitForClose();
		}

		private void Open()
		{
			if (isOpening) return;
			isOpening = true;
			easeToggles.Value[easeId.Value].SetIsOnWithoutNotify(true);
			EventSystem.current.SetSelectedGameObject(easeToggles.Value[easeId.Value].gameObject);
			openMovement.Move(() => panelRoot.anchoredPosition.y, y => panelRoot.anchoredPosition = new Vector2(0, y));
		}

		private void Close()
		{
			isOpening = false;
			closeRcts.CancelAndReset();
			closeMovement.Move(() => panelRoot.anchoredPosition.y, y => panelRoot.anchoredPosition = new Vector2(0, y));
			EventSystem.current.SetSelectedGameObject(null);
		}

		private void WaitForClose()
		{
			UniTask.Delay(closeDelayMs, cancellationToken: closeRcts.Token).ContinueWith(Close).Forget();
		}
	}
}