/** A movement node of a track, representing an editable view corresponding to the track's movement's one MoveItem. */
interface TrackNode {
  readonly track: TrackSnapshot;

  readonly time: T3Time;

  /**
   * The next node's time in the same movement list, or this node's own time
   * if there is no node after it.
   */
  getNextTime(): T3Time;

  /** Returns a copy of this node's movement data (MoveItem). */
  getModel(): MoveItem;
}

declare class TrackEdgeNode implements TrackNode {
  readonly track: TrackSnapshot;

  readonly time: T3Time;

  /** Whether this is the left edge node. */
  readonly isLeft: boolean;

  getNextTime(): T3Time;

  getModel(): MoveItem;
}

declare class TrackDirectNode implements TrackNode {
  readonly track: TrackSnapshot;

  readonly time: T3Time;

  /** Whether this is a position node (vs a width node). */
  readonly isPosition: boolean;

  getNextTime(): T3Time;

  getModel(): MoveItem;
}
