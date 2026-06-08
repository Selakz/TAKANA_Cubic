#nullable enable

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLayer.UI;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Threading;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLayer
{
	public class QuickLayerCheckSystem : HierarchySystem<QuickLayerCheckSystem>
	{
		// Serializable and Public
		[SerializeField] private ViewPoolInstaller quickLayerContentInstaller;
		[SerializeField] private int showInfoTimeMs = 3000;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyNestedRegistrar<LayersInfo?>(manageSystem.LayersInfo, info => new UnionRegistrar(
				new AutoViewPoolRegistrar<LayerComponent>(info!, sorter.ViewPool, true),
				new CustomRegistrar(
					() => info!.OnOrderChanged += sorter.Sort,
					() => info!.OnOrderChanged -= sorter.Sort))),
			new ViewPoolLifetimeRegistrar<LayerComponent>(sorter.ViewPool, handler =>
				new QuickLayerContentRegistrar(handler, sorter.ViewPool[handler]!)),
			new InputRegistrar("TrackEdit", "UpSelectTrackLayer", () =>
			{
				if (selectDataset.Count > 0) return;
				UpSelectTrackLayer();
				ShowInfo();
			}),
			new InputRegistrar("TrackEdit", "DownSelectTrackLayer", () =>
			{
				if (selectDataset.Count > 0) return;
				DownSelectTrackLayer();
				ShowInfo();
			})
		};

		// Private
		[Inject] private TrackLayerManageSystem manageSystem = default!;
		[Inject] private ChartSelectDataset selectDataset = default!;

		private readonly ReusableCancellationTokenSource rcts = new();
		private ListViewAutoSorter<LayerComponent> sorter = default!;

		// Constructor
		[Inject]
		private void Construct([Key("quick-layer-content")] IViewPool<LayerComponent> quickLayerContentPool)
		{
			sorter = new ListViewAutoSorter<LayerComponent>(quickLayerContentPool)
			{
				ListSorter = (a, b) => b.Parent.IndexOf(b.Model.Id) - a.Parent.IndexOf(a.Model.Id)
			};
		}

		public override void SelfInstall(IContainerBuilder builder)
		{
			base.SelfInstall(builder);
			builder.RegisterViewPool<LayerComponent>(quickLayerContentInstaller).Keyed("quick-layer-content");
		}

		// Defined Functions
		private static LayerComponent? GetLayerComponentAt(LayersInfo layersInfo, int index)
		{
			int i = 0;
			foreach (var layer in layersInfo)
			{
				if (i == layersInfo.Count - 1 - index) return layer;
				i++;
			}

			return null;
		}

		private void UpSelectTrackLayer()
		{
			var layersInfo = manageSystem.LayersInfo.Value;
			if (layersInfo is null) return;

			int topSelectedIndex =
				(from layer in layersInfo where layer.Model.IsSelected select layersInfo.IndexOf(layer.Model.Id))
				.Prepend(-1).Max();

			var targetIndex = topSelectedIndex == -1
				? layersInfo.Count - 1
				: Mathf.Min(topSelectedIndex + 1, layersInfo.Count - 1);

			var targetLayer = GetLayerComponentAt(layersInfo, targetIndex);
			if (targetLayer is null) return;

			foreach (var layer in layersInfo)
			{
				bool shouldSelect = layer.Model.Id == targetLayer.Model.Id;
				if (layer.Model.IsSelected != shouldSelect) layer.UpdateModel(info => info.IsSelected = shouldSelect);
			}
		}

		private void DownSelectTrackLayer()
		{
			var layersInfo = manageSystem.LayersInfo.Value;
			if (layersInfo is null) return;

			int bottomSelectedIndex =
				(from layer in layersInfo where layer.Model.IsSelected select layersInfo.IndexOf(layer.Model.Id))
				.Prepend(layersInfo.Count).Min();

			int targetIndex;
			if (bottomSelectedIndex == layersInfo.Count)
			{
				targetIndex = layersInfo.Count - 1;
			}
			else
			{
				targetIndex = Mathf.Max(bottomSelectedIndex - 1, 0);
			}

			var targetLayer = GetLayerComponentAt(layersInfo, targetIndex);
			if (targetLayer is null) return;

			foreach (var layer in layersInfo)
			{
				bool shouldSelect = layer.Model.Id == targetLayer.Model.Id;
				if (layer.Model.IsSelected != shouldSelect) layer.UpdateModel(info => info.IsSelected = shouldSelect);
			}
		}

		private void ShowInfo()
		{
			sorter.ViewPool.DefaultTransform.gameObject.SetActive(true);
			rcts.CancelAndReset();
			UniTask.Delay(showInfoTimeMs, cancellationToken: rcts.Token).ContinueWith(
				() => sorter.ViewPool.DefaultTransform.gameObject.SetActive(false));
		}

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			sorter.ViewPool.DefaultTransform.gameObject.SetActive(false);
		}

		// Inner Class
		private class QuickLayerContentRegistrar : IEventRegistrar
		{
			private readonly LayerComponent layer;
			private readonly QuickLayerContent content;
			private readonly ComponentRegistrar registrar;

			public QuickLayerContentRegistrar(PrefabHandler handler, LayerComponent layer)
			{
				this.layer = layer;
				content = handler.Script<QuickLayerContent>();
				registrar = new ComponentRegistrar(layer, UpdateUI);
			}

			private void UpdateUI(object sender, EventArgs e)
			{
				content.NameText.text = layer.Model.Name;
				content.Indicator.color = layer.Model.Color;
				content.IsSelectedFrame.gameObject.SetActive(layer.Model.IsSelected);
			}

			public void Register()
			{
				UpdateUI(this, EventArgs.Empty);
				registrar.Register();
			}

			public void Unregister()
			{
				registrar.Unregister();
			}
		}
	}
}