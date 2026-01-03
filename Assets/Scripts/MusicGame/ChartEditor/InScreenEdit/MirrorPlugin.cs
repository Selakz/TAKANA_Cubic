#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Select;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Movement;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class MirrorPlugin : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private SequencePriority chartEditPriority = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Mirror", "mirror", chartEditPriority.Value, TrackMirror),
			new InputRegistrar("InScreenEdit", "MirrorCopy", TrackMirrorCopy),
		};

		// Private
		private ChartSelectDataset dataset = default!;

		// Static
		public static void MirrorMovementAction(IMovement<float> movement)
		{
			if (movement is ChartPosMoveList moveList)
			{
				foreach (var item in moveList) item.Value.Position *= -1;
			}
		}

		public static void MirrorTrackAction(ITrack track)
		{
			MirrorMovementAction(track.Movement.Movement1);
			MirrorMovementAction(track.Movement.Movement2);
			(track.Movement.Movement1, track.Movement.Movement2) = (track.Movement.Movement2, track.Movement.Movement1);
		}

		// Defined Functions
		[Inject]
		private void Construct(ChartSelectDataset dataset)
		{
			this.dataset = dataset;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Handlers
		private bool TrackMirror()
		{
			var tracks = dataset.Where(component => component.Model is ITrack);
			var command = new BatchCommand(tracks.Select(track => new UpdateComponentCommand(track,
					model => MirrorTrackAction((model as ITrack)!), model => MirrorTrackAction((model as ITrack)!))),
				"Mirror Tracks");
			CommandManager.Instance.Add(command);
			return true;
		}

		private void TrackMirrorCopy()
		{
			var tracks = dataset.Where(component => component.Model is ITrack).ToArray();
			IEnumerable<ICommand> cloneCommands = tracks.Select(track => new CloneComponentCommand(track));
			IEnumerable<ICommand> mirrorCommands = tracks.Select(track => new UpdateComponentCommand(track,
				model => MirrorTrackAction((model as ITrack)!), model => MirrorTrackAction((model as ITrack)!)));
			var command = new BatchCommand(cloneCommands.Concat(mirrorCommands), "Mirror and Copy Tracks");
			CommandManager.Instance.Add(command);
		}
	}
}