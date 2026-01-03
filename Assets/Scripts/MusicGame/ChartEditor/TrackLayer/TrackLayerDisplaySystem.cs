#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.ChartEditor.TrackLayer.UI;
using T3Framework.Preset.Event;
using T3Framework.Preset.UICollection;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using T3Framework.Static;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLayer
{
	public class TrackLayerDisplaySystem : T3System
	{
		private readonly ListViewManualSorter<LayerComponent> sorter;
		private readonly TrackLayerManageSystem manageSystem;
		private readonly Dictionary<PrefabHandler, LayerContentRegistrar> registrars = new();

		public TrackLayerDisplaySystem(
			[Key("trackLayer")] IViewPool<LayerComponent> viewPool,
			TrackLayerManageSystem manageSystem) : base(true)
		{
			sorter = new(viewPool);
			this.manageSystem = manageSystem;
		}

		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LayersInfo?>(manageSystem.LayersInfo, (_, _) =>
			{
				foreach (var registrar in registrars.Values) registrar.Unregister();
				registrars.Clear();
				sorter.ViewPool.Clear();
				var lastInfo = manageSystem.LayersInfo.LastValue;
				if (lastInfo is not null)
				{
					lastInfo.OnDataAdded -= OnLayerAdded;
					lastInfo.BeforeDataRemoved -= BeforeLayerRemoved;
				}

				var info = manageSystem.LayersInfo.Value;
				if (info is not null)
				{
					info.OnDataAdded += OnLayerAdded;
					info.BeforeDataRemoved += BeforeLayerRemoved;
					foreach (var layer in info)
					{
						if (sorter.ViewPool.Add(layer))
						{
							var handler = sorter.ViewPool[layer]!;
							var registrar = new LayerContentRegistrar(this, handler, info, layer);
							registrar.Register();
							registrars[handler] = registrar;
						}
					}
				}
			})
		};

		private void OnLayerAdded(LayerComponent layer)
		{
			if (manageSystem.LayersInfo.Value is not { } info) return;
			if (sorter.ViewPool.Add(layer))
			{
				var handler = sorter.ViewPool[layer]!;
				var registrar = new LayerContentRegistrar(this, handler, info, layer);
				registrar.Register();
				registrars[handler] = registrar;
			}
		}

		private void BeforeLayerRemoved(LayerComponent layer)
		{
			var handler = sorter.ViewPool[layer]!;
			if (sorter.ViewPool.Remove(layer))
			{
				var registrar = registrars[handler];
				registrar.Unregister();
				registrars.Remove(handler);
			}
		}

		/// <summary> Sets the view, listen the events. </summary>
		private class LayerContentRegistrar : IEventRegistrar
		{
			private readonly LayersInfo layersInfo;
			private readonly LayerComponent layer;
			private readonly EditLayerContent content;
			private readonly IEventRegistrar[] registrars;

			public LayerContentRegistrar(
				TrackLayerDisplaySystem system, PrefabHandler handler, LayersInfo layersInfo, LayerComponent layer)
			{
				this.layersInfo = layersInfo;
				this.layer = layer;
				content = handler.Script<EditLayerContent>();
				registrars = new IEventRegistrar[]
				{
					// Listen to Layer
					new CustomRegistrar(
						() => layer.OnComponentUpdated += UpdateUI, () => layer.OnComponentUpdated -= UpdateUI),
					// Listen to UI
					new ToggleRegistrar(content.IsDecorationToggle,
						isOn =>
						{
							if (!system.manageSystem.TrySetLayerDecoration(layer, isOn))
							{
								content.IsDecorationToggle.SetIsOnWithoutNotify(layer.Model.IsDecoration);
							}
						}),
					new ToggleRegistrar(content.IsSelectedToggle,
						isOn => layer.UpdateModel(info => info.IsSelected = isOn)),
					new InputFieldRegistrar(content.NameInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
						name =>
						{
							if (string.IsNullOrEmpty(name))
							{
								content.NameInputField.SetTextWithoutNotify(layer.Model.Name);
								return;
							}

							layer.UpdateModel(info => info.Name = name);
						}),
					new ButtonRegistrar(content.UpLevelButton, () =>
					{
						system.sorter.SwapUp(layer);
						layersInfo.Sort((a, b) =>
						{
							var ia = system.sorter.ViewPool[a]?.transform.GetSiblingIndex();
							var ib = system.sorter.ViewPool[b]?.transform.GetSiblingIndex();
							if (ia is null || ib is null) return 0;
							return ia.Value.CompareTo(ib.Value);
						});
					}),
					new ButtonRegistrar(content.DownLevelButton, () =>
					{
						system.sorter.SwapDown(layer);
						layersInfo.Sort((a, b) =>
						{
							var ia = system.sorter.ViewPool[a]?.transform.GetSiblingIndex();
							var ib = system.sorter.ViewPool[b]?.transform.GetSiblingIndex();
							if (ia is null || ib is null) return 0;
							return ia.Value.CompareTo(ib.Value);
						});
					}),
					new DoubleClickButtonRegistrar(content.RemoveButton,
						DoubleClickButtonRegistrar.RegisterTarget.First,
						() =>
						{
							var count = system.manageSystem.GetTrackCountOnLayer(layer);
							T3Logger.Log("Notice", $"TrackLayer_Basic_DeleteLayerHint|{count}", T3LogType.Info);
						}),
					new DoubleClickButtonRegistrar(content.RemoveButton,
						DoubleClickButtonRegistrar.RegisterTarget.Second,
						() => system.manageSystem.LayersInfo.Value?.Remove(layer)),
					new PropertyRegistrar<Color>(content.PaletteColor,
						(_, _) => layer.UpdateModel(info => info.Color = content.PaletteColor)),
				};
			}

			private void UpdateUI(object sender, EventArgs e)
			{
				content.IsSelectedToggle.SetIsOnWithoutNotify(layer.Model.IsSelected);
				content.IsDecorationToggle.SetIsOnWithoutNotify(layer.Model.IsDecoration);
				content.NameInputField.SetTextWithoutNotify(layer.Model.Name);
				content.PaletteColor.Value = layer.Model.Color;
				content.RemoveButton.Button.interactable =
					layer.Model.Id != ISingleton<TrackLayerSetting>.Instance.DefaultLayerId;
			}

			public void Register()
			{
				UpdateUI(this, EventArgs.Empty);
				foreach (var registrar in registrars) registrar.Register();
			}

			public void Unregister()
			{
				foreach (var registrar in registrars) registrar.Unregister();
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			foreach (var registrar in registrars.Values) registrar.Unregister();
			registrars.Clear();
			sorter.ViewPool.Clear();
			var info = manageSystem.LayersInfo.Value;
			if (info is not null)
			{
				info.OnDataAdded -= OnLayerAdded;
				info.BeforeDataRemoved -= BeforeLayerRemoved;
			}
		}
	}
}