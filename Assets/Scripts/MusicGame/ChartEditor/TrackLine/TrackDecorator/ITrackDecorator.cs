using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.TrackLine.MoveListDecorator;
using MusicGame.Components.Tracks;
using MusicGame.Components.Tracks.Movement;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.MVC;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine.TrackDecorator
{
	/// <summary> For decorating <see cref="Track"/>'s movement. </summary>
	public interface ITrackDecorator : IController<Track>
	{
		private static readonly LazyPrefab edgeDecoratorPrefab =
			new("Prefabs/EditorUI/TrackLine/TrackEdgeDecorator", "TrackEdgeDecoratorPrefab_OnLoad");

		public static ITrackDecorator Decorate(Transform decoratorRoot, Track track)
		{
			switch (track.Movement)
			{
				case TrackEdgeMovement:
					var edgeDecoratorObject =
						UnityEngine.Object.Instantiate<GameObject>(edgeDecoratorPrefab, decoratorRoot);
					var edgeDecorator = edgeDecoratorObject.GetComponent<TrackEdgeDecorator>();
					edgeDecorator.Init(track);
					return edgeDecorator;
				default:
					// TODO: return fallbackDecorator
					return null;
			}
		}

		public bool IsEditing { get; }

		public IMoveListDecorator MoveListDecorator1 { get; }

		public IMoveListDecorator MoveListDecorator2 { get; }

		/// <summary> Make the decorator disappear and uneditable. </summary>
		public void Disable();

		/// <summary> Make the decorator appear; It will not change whether the decorator is editable. </summary>
		public void Enable();

		public void Uneditable();

		/// <summary> Will do nothing if the decorator is disabled. </summary>
		public void Editable();

		public void UnselectAll();

		ICommand ToLeft();

		ICommand ToRight();

		ICommand ToLeftGrid();

		ICommand ToRightGrid();

		ICommand ToNext();

		ICommand ToPrevious();

		ICommand ToNextBeat();

		ICommand ToPreviousBeat();

		ICommand Create(Vector2 gamePoint);

		ICommand Delete();

		ICommand ChangeNodeState();
	}
}