// Modified from Arcade-Plus: https://github.com/yojohanshinwataikei/Arcade-plus

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using VContainer;
using static UnityEngine.InputSystem.InputActionRebindingExtensions;

namespace App.Rebind
{
	[Serializable]
	public class RebindingItemData : IComponent
	{
		[field: SerializeField]
		public string ActionMapName { get; set; } = string.Empty;

		[field: SerializeField]
		public string ActionName { get; set; } = string.Empty;

		[field: SerializeField]
		public string ActionDisplayName { get; set; } = string.Empty;

		public event EventHandler? OnComponentUpdated;
		public void UpdateNotify() => OnComponentUpdated?.Invoke(this, EventArgs.Empty);
	}

	public class RebindingSystem : HierarchySystem<RebindingSystem>
	{
		// Serializable and Public
		[SerializeField] private string pathInPersistent = "InputBindings.json";
		[SerializeField] private List<RebindingItemData> itemData = default!;
		[SerializeField] private InspectorDictionary<string, Transform> itemViewParents = default!;
		[SerializeField] private PrefabObject prefab = default!;
		[SerializeField] private Transform defaultTransform = default!;

		[SerializeField] private Button resetButton = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolRegistrar<RebindingItemData>(viewPool,
				ViewPoolRegistrar<RebindingItemData>.RegisterTarget.Get, handler =>
				{
					var data = viewPool[handler]!;
					if (itemViewParents.Value.TryGetValue(data.ActionMapName, out var parent))
						handler.transform.SetParent(parent);

					var action = GetInputAction(data.ActionMapName, data.ActionName);
					if (action == null) return;
					var view = handler.Script<RebindingItemView>();
					view.ActionNameText.SetText(data.ActionDisplayName);
					view.CurrentBindingText.SetText(UpdateTextForHotkeyButton(action));
				}),
			new ViewPoolLifetimeRegistrar<RebindingItemData>(viewPool, handler =>
				new ButtonRegistrar(handler.Script<RebindingItemView>().RebindButton, () =>
				{
					var data = viewPool[handler]!;
					var action = GetInputAction(data.ActionMapName, data.ActionName);
					if (action == null) return;
					SetHotkeyRebindingButton(action, UpdateText, UpdateText);
					UpdateText();
					return;

					void UpdateText() =>
						handler.Script<RebindingItemView>().CurrentBindingText.text = UpdateTextForHotkeyButton(action);
				})),
			new ButtonRegistrar(resetButton, () =>
			{
				if (ActionAsset is not null)
				{
					ActionAsset.RemoveAllBindingOverrides();
					var bindingsPath = Path.Combine(Application.persistentDataPath, pathInPersistent);
					File.WriteAllText(bindingsPath, ActionAsset.SaveBindingOverridesAsJson());
					foreach (var data in viewPool)
					{
						var handler = viewPool[data]!;
						var action = GetInputAction(data.ActionMapName, data.ActionName);
						if (action == null) return;
						handler.Script<RebindingItemView>().CurrentBindingText.text = UpdateTextForHotkeyButton(action);
					}
				}
			})
		};

		// Private
		[Inject] private readonly IObjectResolver resolver = default!;

		private static InputActionAsset? ActionAsset => ISingleton<InputManager>.Instance.ActionAsset;
		private IViewPool<RebindingItemData> viewPool = default!;

		private RebindingOperation? rebindingOperation;

		public void SetHotkeyRebindingButton(InputAction action, Action onComplete, Action onCancel)
		{
			EventSystem.current.SetSelectedGameObject(null); // clean focus so won't restart rebinding after complete
			rebindingOperation?.Cancel(); // cleanup old operation

			// start new operation
			rebindingOperation = action.PerformInteractiveRebinding()
				.WithExpectedControlType<ButtonControl>()
				.WithControlsHavingToMatchPath("<Keyboard>")
				.WithControlsExcluding("<Keyboard>/leftAlt")
				.WithControlsExcluding("<Keyboard>/leftCtrl")
				.WithControlsExcluding("<Keyboard>/leftShift")
				.WithControlsExcluding("<Keyboard>/rightAlt")
				.WithControlsExcluding("<Keyboard>/rightCtrl")
				.WithControlsExcluding("<Keyboard>/rightShift")
				.WithControlsExcluding("<Keyboard>/alt")
				.WithControlsExcluding("<Keyboard>/ctrl")
				.WithControlsExcluding("<Keyboard>/shift")
				.WithControlsExcluding("<Keyboard>/anyKey")
				.WithControlsExcluding("<Keyboard>/escape")
				.WithControlsExcluding("<Pointer>")
				.WithCancelingThrough(Keyboard.current.escapeKey)
				.WithMatchingEventsBeingSuppressed()
				.OnMatchWaitForAnother(float.PositiveInfinity)
				.OnPotentialMatch(operation =>
				{
					if (operation.candidates.Count > 0) operation.Complete();
				})
				.OnApplyBinding((_, bindingPath) =>
				{
					if (!IsHotKey(action))
					{
						Debug.LogError("The action for the rebinding button is not hotkey: not a hotkey binding");
						return;
					}

					bool hasCtrl = Keyboard.current.ctrlKey.isPressed;
					bool hasAlt = Keyboard.current.altKey.isPressed;
					bool hasShift = Keyboard.current.shiftKey.isPressed;
					bool? needModifier1 = null;
					bool? needModifier2 = null;
					bool? needModifier3 = null;
					for (int i = 1; i < 5; i++)
					{
						InputBinding binding = action.bindings[i];
						if (binding.name == "key")
						{
							action.ApplyBindingOverride(i, bindingPath);
							continue;
						}

						bool? needModifier = binding.effectivePath switch
						{
							"<Keyboard>/ctrl" => hasCtrl,
							"<Keyboard>/alt" => hasAlt,
							"<Keyboard>/shift" => hasShift,
							_ => null
						};

						switch (binding.name)
						{
							case "modifier1":
								needModifier1 = needModifier;
								break;
							case "modifier2":
								needModifier2 = needModifier;
								break;
							case "modifier3":
								needModifier3 = needModifier;
								break;
							default:
								return;
						}
					}

					List<string> param = new()
					{
						$"needModifier1={needModifier1.ToString().ToLower()}",
						$"needModifier2={needModifier2.ToString().ToLower()}",
						$"needModifier3={needModifier3.ToString().ToLower()}"
					};
					InputBinding compositeBinding = action.bindings[0];
					// Note: LoadBindingOverridesFromJson will apply empty string and SaveBindingOverridesAsJson will save null to empty string
					// so as a workaround here we set the overridePath to path
					compositeBinding.overridePath = compositeBinding.path;
					compositeBinding.overrideInteractions = $"HotKey({string.Join(",", param)})";
					action.ApplyBindingOverride(0, compositeBinding);
				})
				.OnComplete(_ =>
				{
					var bindingsPath = Path.Combine(Application.persistentDataPath, pathInPersistent);
					File.WriteAllText(bindingsPath, ActionAsset.SaveBindingOverridesAsJson());
					CleanupCurrentOperation();
					onComplete.Invoke();
				})
				.OnCancel(_ =>
				{
					CleanupCurrentOperation();
					onCancel.Invoke();
				})
				.Start();
		}

		public string? UpdateTextForHotkeyButton(InputAction action)
		{
			if (!IsHotKey(action))
			{
				Debug.LogError("The action for the rebinding button is not hotkey: not a hotkey binding");
				return null;
			}

			if (rebindingOperation?.action == action) return "<?>";

			bool needModifier1 = false;
			bool needModifier2 = false;
			bool needModifier3 = false;
			string interaction = action.bindings[0].effectiveInteractions;
			// Note: this depends on conventions on the effectivePath format
			if (interaction.Contains('('))
			{
				int start = interaction.IndexOf('(');
				int end = interaction.IndexOf(')');
				string param = interaction.Substring(start + 1, end - start - 1);
				foreach (string keyValue in param.Split(","))
				{
					int separator = keyValue.IndexOf('=');
					string paramKey = keyValue[..separator];
					string paramValue = keyValue[(separator + 1)..];
					if (paramKey == "needModifier1") needModifier1 = paramValue == "true";
					else if (paramKey == "needModifier2") needModifier2 = paramValue == "true";
					else if (paramKey == "needModifier3") needModifier3 = paramValue == "true";
				}
			}

			bool? hasCtrl = null;
			bool? hasAlt = null;
			bool? hasShift = null;
			string? key = null;
			for (int i = 1; i < 5; i++)
			{
				InputBinding binding = action.bindings[i];
				if (binding.name == "key")
				{
					key = binding.ToDisplayString();
					continue;
				}

				bool? needModifier = binding.name switch
				{
					"modifier1" => needModifier1,
					"modifier2" => needModifier2,
					"modifier3" => needModifier3,
					_ => null
				};

				if (binding.effectivePath == "<Keyboard>/ctrl") hasCtrl = needModifier;
				else if (binding.effectivePath == "<Keyboard>/alt") hasAlt = needModifier;
				else if (binding.effectivePath == "<Keyboard>/shift") hasShift = needModifier;
			}

			if (hasCtrl == null || hasAlt == null || hasShift == null || key == null)
			{
				Debug.LogError("The action for the rebinding button is not hotkey: missing binding component");
				return null;
			}

			string result = key;
			if (hasShift.Value) result = "Shift+" + result;
			if (hasAlt.Value) result = "Alt+" + result;
			if (hasCtrl.Value) result = "Ctrl+" + result;
			return result;
		}

		private static InputAction? GetInputAction(string mapName, string actionName) =>
			ActionAsset?.FindActionMap(mapName)?.FindAction(actionName);

		private void CleanupCurrentOperation()
		{
			rebindingOperation?.Dispose();
			rebindingOperation = null;
		}

		private static bool IsHotKey(InputAction action) =>
			action.bindings.Count == 5 &&
			action.bindings[0].isComposite &&
			action.bindings[0].GetNameOfComposite() == "KeyWithModifiers";

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			// Load Bindings
			var bindingsPath = Path.Combine(Application.persistentDataPath, pathInPersistent);
			if (File.Exists(bindingsPath))
			{
				var bindingsJson = File.ReadAllText(bindingsPath);
				ActionAsset?.LoadBindingOverridesFromJson(bindingsJson);
			}

			viewPool = new ViewPool<RebindingItemData>(resolver, prefab, defaultTransform);
		}

		protected override void OnEnable()
		{
			base.OnEnable();
			foreach (var data in itemData) viewPool.Add(data);
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			viewPool.Clear();
		}
	}
}