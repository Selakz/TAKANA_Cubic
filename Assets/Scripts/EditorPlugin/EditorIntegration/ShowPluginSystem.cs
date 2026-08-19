#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using EditorPlugin.EditorIntegration.UI;
using EditorPlugin.PluginSystem;
using MusicGame.ChartEditor.Message;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace EditorPlugin.EditorIntegration
{
	public class ShowPluginSystem : HierarchySystem<ShowPluginSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new AutoViewPoolRegistrar<PluginComponent>(dataset, viewPool, true),
			new ViewPoolLifetimeRegistrar<PluginComponent>(viewPool,
				handler => new EditPluginContentRegistrar(handler.Script<EditPluginContent>(), viewPool[handler]!,
					bindInputService, manageService, messageBox))
		};

		// Private
		[Inject] private IDataset<PluginComponent> dataset = default!;
		[Inject] private IViewPool<PluginComponent> viewPool = default!;
		[Inject] private IPluginBindInputService bindInputService = default!;
		[Inject] private MessageBox messageBox = default!;
		[Inject] private IPluginManageService manageService = default!;
	}

	public class EditPluginContentRegistrar : CompositeRegistrar
	{
		private readonly EditPluginContent content;
		private readonly PluginComponent component;
		private readonly IPluginBindInputService bindInputService;
		private readonly IPluginManageService manageService;
		private readonly MessageBox messageBox;

		private readonly Dictionary<EditPluginParamContent, IEventRegistrar> paramBindings = new();
		private readonly Dictionary<EditPluginHotkeyContent, IEventRegistrar> hotkeyBindings = new();
		private bool shouldUpdateUI;

		private PluginManifest Manifest => component.Model.Manifest;

		public EditPluginContentRegistrar(EditPluginContent content, PluginComponent component,
			IPluginBindInputService bindInputService, IPluginManageService manageService, MessageBox messageBox)
		{
			this.content = content;
			this.component = component;
			this.bindInputService = bindInputService;
			this.manageService = manageService;
			this.messageBox = messageBox;
		}

		protected override IEventRegistrar[] InnerRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<EventHandler>(
				e => component.OnComponentUpdated += e,
				e => component.OnComponentUpdated -= e,
				(_, _) => UpdateUI()),
			new ButtonRegistrar(content.ExecuteButton,
				() => component.Model.Execute()),
			new ButtonRegistrar(content.DetailButton,
				() => messageBox.ShowConfirm(component.Model.Manifest.Description.Value, null, false)),
			new ButtonRegistrar(content.CreateNewHotkeyButton,
				() => bindInputService.BindInput(component, HotkeyPreset.FromPlugin(component.Model, 0), true)),
			new ButtonRegistrar(content.DeleteButton, () =>
			{
				if (!manageService.DeletePlugin(component))
				{
					T3Logger.Log("Notice",
						$"EditorPlugin_DeletePluginFailed|{component.Model.Manifest.Name.Value}", T3LogType.Error);
				}
				else
				{
					T3Logger.Log("Notice",
						$"EditorPlugin_DeletePluginSuccess|{component.Model.Manifest.Name.Value}", T3LogType.Success);
				}
			})
		};

		public void UpdateUI()
		{
			if (!shouldUpdateUI) return;
			shouldUpdateUI = false;
			try
			{
				content.NameText.text = component.Model.Manifest.Name.Value;
				content.UpdateParams(Manifest.Params, Manifest.HotkeyPresets);
				ReconcileParamBindings();
				ReconcileHotkeyBindings();
			}
			finally
			{
				shouldUpdateUI = true;
			}
		}

		protected override void Initialize()
		{
			shouldUpdateUI = true;
			UpdateUI();
			foreach (var preset in component.Model.Manifest.HotkeyPresets)
				bindInputService.BindInput(component, preset, false);
		}

		protected override void Deinitialize()
		{
			shouldUpdateUI = false;
			UnbindAll();
			content.UpdateParams(Array.Empty<PluginParam>(), Array.Empty<HotkeyPreset>());
			foreach (var preset in component.Model.Manifest.HotkeyPresets.ToList())
				bindInputService.UnbindInput(component, preset, false);
		}

		private void ReconcileParamBindings()
		{
			foreach (var (paramContent, registrar) in paramBindings.ToList())
			{
				if (content.ParamContents.Values.Contains(paramContent)) continue;
				registrar.Unregister();
				paramBindings.Remove(paramContent);
			}

			foreach (var (id, paramContent) in content.ParamContents.ToList())
			{
				if (paramBindings.ContainsKey(paramContent)) continue;
				var wrapper = component.Model.Params[id];
				paramContent.Value.Value = wrapper.value;
				paramBindings[paramContent] = BindPluginParam(paramContent, wrapper);
				paramBindings[paramContent].Register();
			}
		}

		private void ReconcileHotkeyBindings()
		{
			foreach (var (hotKeyContent, registrar) in hotkeyBindings.ToList())
			{
				if (content.HotkeyContents.Values.Contains(hotKeyContent)) continue;
				registrar.Unregister();
				hotkeyBindings.Remove(hotKeyContent);
			}

			foreach (var (preset, hotKeyContent) in content.HotkeyContents.ToList())
			{
				hotKeyContent.NameText.text = $"HotKey {preset.Hotkey}";
				int maxIndex = Mathf.Max(0, hotKeyContent.HotkeyDropdown.options.Count - 1);
				hotKeyContent.HotkeyDropdown.SetValueWithoutNotify(Mathf.Clamp(preset.Hotkey, 0, maxIndex));
				if (hotkeyBindings.ContainsKey(hotKeyContent)) continue;
				hotkeyBindings[hotKeyContent] = BindHotkey(preset, hotKeyContent);
				hotkeyBindings[hotKeyContent].Register();
			}
		}

		private static IEventRegistrar BindPluginParam(EditPluginParamContent paramContent, PluginParamWrapper wrapper)
		{
			return new UnionRegistrar(
				new PropertyRegistrar<object>(paramContent.Value, value =>
				{
					try
					{
						wrapper.value = value;
					}
					catch (FormatException)
					{
						T3Logger.Log("Notice", "EditorPlugin_ParamFormatError", T3LogType.Error);
					}
				}), CustomRegistrar.Generic<Action>(
					e => wrapper.OnValueChanged += e,
					e => wrapper.OnValueChanged -= e,
					() => paramContent.Value.Value = wrapper.value));
		}

		private IEventRegistrar BindHotkey(HotkeyPreset preset, EditPluginHotkeyContent hotkeyContent)
		{
			var registrars = new List<IEventRegistrar>
			{
				new ButtonRegistrar(hotkeyContent.DeleteButton,
					() => bindInputService.UnbindInput(component, preset, true)),
				new DropdownRegistrar(hotkeyContent.HotkeyDropdown, value =>
				{
					preset.Hotkey = value;
					bindInputService.BindInput(component, preset, true);
					hotkeyContent.NameText.text = $"HotKey {preset.Hotkey}";
				})
			};

			foreach (var (id, paramContent) in hotkeyContent.ParamContents)
			{
				registrars.Add(new PropertyRegistrar<object>(paramContent.Value, value =>
				{
					preset.SetParam(id, value);
					bindInputService.BindInput(component, preset, true);
				}));

				if (preset.Params.TryGetValue(id, out var storedValue))
				{
					paramContent.Value.Value = storedValue;
				}
			}

			return new UnionRegistrar(registrars.ToArray());
		}

		private void UnbindAll()
		{
			foreach (var registrar in paramBindings.Values) registrar.Unregister();
			paramBindings.Clear();
			foreach (var registrar in hotkeyBindings.Values) registrar.Unregister();
			hotkeyBindings.Clear();
		}
	}
}