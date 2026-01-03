#nullable enable

// @formatter:off

global using EdgeComponent = T3Framework.Runtime.ECS.DerivedComponent<MusicGame.ChartEditor.Decoration.Track.MovementLocator<MusicGame.Models.Track.Movement.TrackEdgeMovement>, MusicGame.Models.Track.Movement.TrackEdgeMovement>;
global using EdgeDataset = T3Framework.Runtime.ECS.DerivedDataset<MusicGame.Gameplay.Chart.ChartComponent, MusicGame.Models.Track.Movement.TrackEdgeMovement, MusicGame.ChartEditor.Decoration.Track.MovementLocator<MusicGame.Models.Track.Movement.TrackEdgeMovement>>;

global using EdgePMLComponent = T3Framework.Runtime.ECS.DerivedComponent<MusicGame.ChartEditor.Decoration.Track.EdgeSideMovementLocator<MusicGame.Models.Track.Movement.ChartPosMoveList>, MusicGame.Models.Track.Movement.ChartPosMoveList>;
global using EdgePMLDataset = T3Framework.Runtime.ECS.DerivedDataset<T3Framework.Runtime.ECS.DerivedComponent<MusicGame.ChartEditor.Decoration.Track.MovementLocator<MusicGame.Models.Track.Movement.TrackEdgeMovement>, MusicGame.Models.Track.Movement.TrackEdgeMovement>, MusicGame.Models.Track.Movement.ChartPosMoveList, MusicGame.ChartEditor.Decoration.Track.EdgeSideMovementLocator<MusicGame.Models.Track.Movement.ChartPosMoveList>>;

global using EdgeNodeComponent = T3Framework.Runtime.ECS.DerivedComponent<MusicGame.ChartEditor.Decoration.Track.EdgeSideMoveItemLocator, T3Framework.Static.Movement.IPositionMoveItem<float>>;
global using EdgeNodeDataset = T3Framework.Runtime.ECS.DerivedDataset<T3Framework.Runtime.ECS.DerivedComponent<MusicGame.ChartEditor.Decoration.Track.EdgeSideMovementLocator<MusicGame.Models.Track.Movement.ChartPosMoveList>, MusicGame.Models.Track.Movement.ChartPosMoveList>, T3Framework.Static.Movement.IPositionMoveItem<float>, MusicGame.ChartEditor.Decoration.Track.EdgeSideMoveItemLocator>;

// @formatter:on