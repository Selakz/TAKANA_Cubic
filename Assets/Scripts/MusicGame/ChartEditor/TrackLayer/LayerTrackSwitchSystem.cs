#nullable enable

using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Track;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLayer
{
	public class LayerTrackSwitchSystem : HierarchySystem<LayerTrackSwitchSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority moduleId = default!;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<int>(moduleInfo.CurrentModule, id => IsEnabled = moduleId == id)
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "ToLeft",
				() => SwitchToNextTrack(true)),
			new InputRegistrar("InScreenEdit", "ToRight",
				() => SwitchToNextTrack(false))
		};

		// Private
		private ModuleInfo moduleInfo = default!;
		private IGameAudioPlayer music = default!;
		private ChartSelectDataset selectDataset = default!;

		private IReadOnlyDataset<ChartComponent> viewPoolSubSet = default!;

		// Constructor
		[Inject]
		private void Construct(
			ModuleInfo moduleInfo,
			IGameAudioPlayer music,
			ChartSelectDataset selectDataset,
			[Key("stage")] IViewPool<ChartComponent> viewPool)
		{
			this.moduleInfo = moduleInfo;
			this.music = music;
			this.selectDataset = selectDataset;

			viewPoolSubSet = viewPool.SubDataset(T3ChartClassifier.Instance, T3Flag.Track);
		}

		// Defined Functions
		private void SwitchToNextTrack(bool isLeft)
		{
			if (selectDataset.Count != 1 ||
			    selectDataset.CurrentSelecting.Value is not { Model: ITrack model } track ||
			    !viewPoolSubSet.Contains(track) ||
			    track.GetLayerInfo() is not { IsSelected: true } info) return;

			ChartComponent? target = null;
			float minDistance = float.MaxValue;
			var basePosition = model.Movement.GetPos(music.ChartTime);
			foreach (var component in viewPoolSubSet)
			{
				if (component.Model is not ITrack otherModel ||
				    component.GetLayerInfo()?.Id != info.Id) continue;

				var position = otherModel.Movement.GetPos(music.ChartTime);
				var distance = Mathf.Abs(position - basePosition);
				if (((isLeft && position < basePosition) || (!isLeft && position > basePosition)) &&
				    distance < minDistance)
				{
					target = component;
					minDistance = distance;
				}
			}

			if (target is not null)
			{
				selectDataset.Clear();
				selectDataset.Add(target);
			}
		}
	}
}