#nullable enable

using System;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.EditPanel.Commands;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLayer;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Log;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.ChartEditor.EditPanel.Track
{
	public class EditTrackContent : MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public EditNameTitle NameTitle { get; set; } = default!;

		[field: SerializeField]
		public Button UnselectButton { get; set; } = default!;

		[field: SerializeField]
		public Button DeleteButton { get; set; } = default!;

		[field: SerializeField]
		public Transform ContentRoot { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField TimeStartInputField { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField TimeEndInputField { get; set; } = default!;

		[field: SerializeField]
		public TMP_Dropdown LayerDropdown { get; set; } = default!;

		// Private
		private PrefabHandler handler = default!;

		[Inject]
		private void Construct()
		{
			handler = GetComponent<PrefabHandler>();
			handler.OnPluginAdded += OnPluginAdded;
		}

		// Temp
		private void OnPluginAdded(string id)
		{
			var plugin = handler[id];
			plugin!.transform.SetParent(ContentRoot, false);
		}

		// System Functions
		void OnDestroy() => handler.OnPluginAdded -= OnPluginAdded;
	}

	public class TrackContentRegistrar : IEventRegistrar
	{
		private readonly EditTrackContent editTrackContent;
		private readonly ChartComponent component;
		private readonly TrackLayerManageSystem manageSystem;

		private readonly IEventRegistrar[] registrars;
		private int[]? layerIds;

		public TrackContentRegistrar(
			EditTrackContent editTrackContent,
			ChartComponent component,
			ChartSelectDataset dataset,
			TrackLayerManageSystem manageSystem)
		{
			this.editTrackContent = editTrackContent;
			this.component = component;
			this.manageSystem = manageSystem;

			registrars = new IEventRegistrar[]
			{
				CustomRegistrar.Generic<EventHandler>(
					e => component.OnComponentUpdated += e,
					e => component.OnComponentUpdated -= e,
					(_, _) => UpdateUI()),
				new NameTitleRegistrar(editTrackContent.NameTitle, component),
				new ButtonRegistrar(editTrackContent.UnselectButton,
					() => dataset.Remove(component)),
				new ButtonRegistrar(editTrackContent.DeleteButton,
					() =>
					{
						var command = new DeleteComponentCommand(component);
						CommandManager.Instance.Add(command);
					}),
				new InputFieldRegistrar(editTrackContent.TimeStartInputField,
					InputFieldRegistrar.RegisterTarget.OnEndEdit,
					content =>
					{
						ITrack track = (ITrack)component.Model;
						if (int.TryParse(content, out int newStart) && component.IsNewTimeMinValid(newStart))
						{
							if (newStart == track.TimeStart) return;
							var command = new UpdateTrackTimeStartCommand(component, newStart);
							if (!command.SetInit()) return;
							CommandManager.Instance.Add(command);
						}
						else
						{
							T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
							editTrackContent.TimeStartInputField.SetTextWithoutNotify(track.TimeStart.ToString());
						}
					}),
				new InputFieldRegistrar(editTrackContent.TimeEndInputField,
					InputFieldRegistrar.RegisterTarget.OnEndEdit,
					content =>
					{
						ITrack track = (ITrack)component.Model;
						if (int.TryParse(content, out int newEnd) && component.IsNewTimeMaxValid(newEnd))
						{
							if (newEnd == track.TimeEnd) return;
							var command = new UpdateTrackTimeEndCommand(component, newEnd);
							if (!command.SetInit()) return;
							CommandManager.Instance.Add(command);
						}
						else
						{
							T3Logger.Log("Notice", "Edit_Fail", T3LogType.Warn);
							editTrackContent.TimeEndInputField.SetTextWithoutNotify(track.TimeEnd.ToString());
						}
					}),
				new DropdownRegistrar(editTrackContent.LayerDropdown,
					newValue =>
					{
						var model = (ITrack)component.Model;
						var oldLayer = model.GetLayerId();
						var newLayer = layerIds![newValue];
						if (component.BelongingChart?.GetsLayersInfo() is not { } layersInfo) return;
						if (layersInfo[newLayer] is not { } newInfo) return;
						if (newInfo.IsDecoration && component.Children.Count > 0)
						{
							T3Logger.Log("Notice", "TrackLayer_Decoration_SetDecorationFail", T3LogType.Warn);
							editTrackContent.LayerDropdown.SetValueWithoutNotify(Array.IndexOf(layerIds, oldLayer));
							return;
						}

						CommandManager.Instance.Add(new UpdateComponentCommand(
							component,
							_ => model.SetLayer(newLayer),
							_ => model.SetLayer(oldLayer)
						));
					}),
				new PropertyNestedRegistrar<LayersInfo?>(
					manageSystem.LayersInfo,
					layersInfo => new UnionRegistrar(
						CustomRegistrar.Generic<Action<LayerComponent>>(
							e => layersInfo!.OnDataAdded += e,
							e => layersInfo!.OnDataAdded -= e,
							_ => SetLayer(layersInfo!)),
						CustomRegistrar.Generic<Action<LayerComponent>>(
							e => layersInfo!.BeforeDataRemoved += e,
							e => layersInfo!.BeforeDataRemoved -= e,
							_ => SetLayer(layersInfo!)),
						CustomRegistrar.Generic<Action<LayerComponent>>(
							e => layersInfo!.OnDataUpdated += e,
							e => layersInfo!.OnDataUpdated -= e,
							_ => SetLayer(layersInfo!))
					))
			};
		}

		private void UpdateUI()
		{
			var track = (ITrack)component.Model;
			editTrackContent.TimeStartInputField.SetTextWithoutNotify(track.TimeStart.ToString());
			editTrackContent.TimeEndInputField.SetTextWithoutNotify(track.TimeEnd.ToString());
			if (manageSystem.LayersInfo.Value is { } info) SetLayer(info);
			editTrackContent.transform.localScale = Vector3.one;
		}

		private void SetLayer(LayersInfo layersInfo)
		{
			layerIds = editTrackContent.LayerDropdown.SetOptions(
				layersInfo.Select(layer => layer.Model.Id).ToList(),
				id => layersInfo[id]?.Name ?? string.Empty);

			var layer = ((ITrack)component.Model).GetLayerId();
			for (var i = 0; i < layerIds.Length; i++)
			{
				var layerId = layerIds[i];
				if (layer == layerId) editTrackContent.LayerDropdown.SetValueWithoutNotify(i);
			}
		}

		public void Register()
		{
			UpdateUI();
			foreach (var registrar in registrars) registrar.Register();
		}

		public void Unregister()
		{
			foreach (var registrar in registrars) registrar.Unregister();
		}
	}
}