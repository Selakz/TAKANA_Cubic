#nullable enable

// @formatter:off

global using EdgeNodeComponent = T3Framework.Runtime.ECS.DerivedComponent<MusicGame.ChartEditor.Decoration.Track.EdgeSideMoveItemLocator, T3Framework.Static.Movement.IPositionMoveItem<float>>;
global using EdgeNodeDataset = T3Framework.Runtime.ECS.DerivedDataset<T3Framework.Runtime.ECS.DerivedComponent<MusicGame.ChartEditor.Decoration.Track.EdgeSideMovementLocator<MusicGame.Models.Track.Movement.ChartPosMoveList>, MusicGame.Models.Track.Movement.ChartPosMoveList>, T3Framework.Static.Movement.IPositionMoveItem<float>, MusicGame.ChartEditor.Decoration.Track.EdgeSideMoveItemLocator>;

global using DirectNodeComponent = T3Framework.Runtime.ECS.DerivedComponent<MusicGame.ChartEditor.Decoration.Track.DirectSideMoveItemLocator, T3Framework.Static.Movement.IPositionMoveItem<float>>;
global using DirectNodeDataset = T3Framework.Runtime.ECS.DerivedDataset<T3Framework.Runtime.ECS.DerivedComponent<MusicGame.ChartEditor.Decoration.Track.DirectSideMovementLocator<MusicGame.Models.Track.Movement.ChartPosMoveList>, MusicGame.Models.Track.Movement.ChartPosMoveList>, T3Framework.Static.Movement.IPositionMoveItem<float>, MusicGame.ChartEditor.Decoration.Track.DirectSideMoveItemLocator>;

// @formatter:on
