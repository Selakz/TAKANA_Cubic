/** A track as plain data (Model). Modifying it does not affect the chart until injected via `ChartSnapshot.addTrack`. */
declare class TrackModel implements ComponentModel {
  constructor(timeStart: T3Time, timeEnd: T3Time, movement: TrackMovement);

  timeStart: T3Time;

  timeEnd: T3Time;

  movement: TrackMovement;

  get timeMin(): T3Time;

  get timeMax(): T3Time;

  nudge(distance: T3Time): void;

  /** Moves the whole track horizontally by the given offset. */
  shift(offset: number): void;
}

declare class TrackSnapshot implements ComponentSnapshot {
  readonly id: Wrapper<number>;

  readonly name: Wrapper<string | null>;

  /**
   * Wrapper of the track movement. Use it for queries and single-node edits
   * (see `TrackMovementWrapper`), or `getModel()` for an editable copy.
   */
  readonly movement: TrackMovementWrapper;

  /** The notes on this track. */
  readonly notes: Iterable<NoteSnapshot>;

  /**
   * The layer this track belongs to (read-only snapshot).
   * Use `setLayer` to change it.
   */
  get layer(): Layer;

  /**
   * Sets the track's layer. Needs `commit()`.
   * NOTE: passing a non-existent layer id will make later `layer` reads throw.
   */
  setLayer(id: number): void;

  get timeMin(): T3Time;

  get timeMax(): T3Time;

  nudge(distance: T3Time): void;

  /** Moves the whole track horizontally. */
  shift(offset: number): void;

  /** Returns an independent Model copy; edits don't affect the chart. */
  getModel(): TrackModel;
}

/** A track's movement data as plain data (Model). */
interface TrackMovement {
  getPosition(time: T3Time): number;

  getWidth(time: T3Time): number;

  getLeftPosition(time: T3Time): number;

  getRightPosition(time: T3Time): number;

  nudge(distance: T3Time): void;

  shift(offset: number): void;

  /** Make this movement appears with the given position and width at the given time, without caring which type it is. */
  insert(time: T3Time, position: number, width: number): void;
}

/**
 * Wrapper around a track's movement for queries and single-node edits.
 */
interface TrackMovementWrapper {
  getPosition(time: T3Time): number;

  getWidth(time: T3Time): number;

  getLeftPosition(time: T3Time): number;

  getRightPosition(time: T3Time): number;

  insert(time: T3Time, position: number, width: number): void;

  /** Returns an editable Model copy of the movement. */
  getModel(): TrackMovement;
}

/** Edge-mode movement, driven by left/right edge move lists. */
declare class TrackEdgeMovement implements TrackMovement {
  constructor(leftMoveList: MoveList, rightMoveList: MoveList);

  readonly leftMoveList: MoveList;
  readonly rightMoveList: MoveList;

  getPosition(time: T3Time): number;

  getWidth(time: T3Time): number;

  getLeftPosition(time: T3Time): number;

  getRightPosition(time: T3Time): number;

  nudge(distance: T3Time): void;

  shift(offset: number): void;

  insert(time: T3Time, position: number, width: number): void;
}

declare class TrackEdgeMovementWrapper implements TrackMovementWrapper {
  getPosition(time: T3Time): number;

  getWidth(time: T3Time): number;

  getLeftPosition(time: T3Time): number;

  getRightPosition(time: T3Time): number;

  /**
   * Returns the node at the given time on the given side, or undefined.
   * isLeft: whether to look at the left edge (vs the right edge).
   */
  get(time: T3Time, isLeft: boolean): MoveItem | undefined;

  /**
   * Sets/inserts a node at the given time on the given side.
   * isLeft: whether to edit the left edge (vs the right edge).
   */
  set(time: T3Time, item: MoveItem, isLeft: boolean): boolean;
  /**
   * Deletes the node at the given time on the given side.
   * isLeft: whether to edit the left edge (vs the right edge).
   */
  delete(time: T3Time, isLeft: boolean): boolean;

  insert(time: T3Time, position: number, width: number): void;

  getModel(): TrackEdgeMovement;
}

/** Direct-mode movement, driven by a position move list and a width move list. */
declare class TrackDirectMovement implements TrackMovement {
  constructor(positionMoveList: MoveList, widthMoveList: MoveList);

  readonly positionMoveList: MoveList;
  readonly widthMoveList: MoveList;

  getPosition(time: T3Time): number;

  getWidth(time: T3Time): number;

  getLeftPosition(time: T3Time): number;

  getRightPosition(time: T3Time): number;

  nudge(distance: T3Time): void;

  shift(offset: number): void;

  insert(time: T3Time, position: number, width: number): void;
}

declare class TrackDirectMovementWrapper implements TrackMovementWrapper {
  getPosition(time: T3Time): number;

  getWidth(time: T3Time): number;

  getLeftPosition(time: T3Time): number;

  getRightPosition(time: T3Time): number;

  /**
   * Returns the node at the given time for the given kind, or undefined.
   * isPosition: whether to look at the position list (vs the width list).
   */
  get(time: T3Time, isPosition: boolean): MoveItem | undefined;

  /**
   * Sets/inserts a node at the given time for the given kind.
   * isPosition: whether to edit the position list (vs the width list).
   */
  set(time: T3Time, item: MoveItem, isPosition: boolean): boolean;

  /**
   * Deletes the node at the given time for the given kind.
   * isPosition: whether to edit the position list (vs the width list).
   */
  delete(time: T3Time, isPosition: boolean): boolean;

  insert(time: T3Time, position: number, width: number): void;

  getModel(): TrackDirectMovement;
}

/** A single movement node. */
declare interface MoveItem {
  /** The horizontal position of this node. */
  position: number;

  /**
   * Interpolates the position at targetTime, between this node (at thisTime)
   * and the next node (at nextTime with nextPosition).
   */
  getPosition(
    thisTime: T3Time,
    targetTime: T3Time,
    nextTime: T3Time,
    nextPosition: number,
  ): number;

  clone(): MoveItem;
}

declare enum Eases {
  Unmove,
  Linear,
  InSine,
  OutSine,
  InOutSine,
  OutInSine,
  InQuad,
  OutQuad,
  InOutQuad,
  OutInQuad,
  InCubic,
  OutCubic,
  InOutCubic,
  OutInCubic,
  InQuart,
  OutQuart,
  InOutQuart,
  OutInQuart,
  InQuint,
  OutQuint,
  InOutQuint,
  OutInQuint,
  InExpo,
  OutExpo,
  InOutExpo,
  OutInExpo,
  InCirc,
  OutCirc,
  InOutCirc,
  OutInCirc,
  InBack,
  OutBack,
  InOutBack,
  OutInBack,
  InElastic,
  OutElastic,
  InOutElastic,
  OutInElastic,
  InBounce,
  OutBounce,
  InOutBounce,
  OutInBounce,
}

/** A node that eases toward the next node. */
declare class EaseMoveItem implements MoveItem {
  constructor(position: number, ease: Eases);

  position: number;
  ease: Eases;

  getPosition(
    thisTime: T3Time,
    targetTime: T3Time,
    nextTime: T3Time,
    nextPosition: number,
  ): number;

  clone(): MoveItem;
}

/** A node that uses a cubic bezier curve toward the next node. */
declare class BezierMoveItem implements MoveItem {
  constructor(
    position: number,
    startTimeFactor: number,
    startPositionFactor: number,
    endTimeFactor: number,
    endPositionFactor: number,
  );

  position: number;
  /** Time-curve control factor at segment start (expected in [0, 1]). */
  startTimeFactor: number;
  /** Position-curve control factor at segment start (expected in [0, 1]). */
  startPositionFactor: number;
  /** Time-curve control factor at segment end (expected in [0, 1]). */
  endTimeFactor: number;
  /** Position-curve control factor at segment end (expected in [0, 1]). */
  endPositionFactor: number;

  getPosition(
    thisTime: T3Time,
    targetTime: T3Time,
    nextTime: T3Time,
    nextPosition: number,
  ): number;
  clone(): MoveItem;
}

/**
 * An editable list of movement nodes (Model).
 * Edits apply to this object immediately; inject it into the chart via a TrackModel.
 */
declare class MoveList {
  /** Read-only view of the nodes. */
  readonly items: ReadonlyMap<T3Time, MoveItem>;

  constructor(items?: ReadonlyMap<T3Time, MoveItem>);

  /** Sets/inserts a node at the given time. */
  set(time: T3Time, item: MoveItem): boolean;

  /** Deletes the node at the given time. */
  delete(time: T3Time): boolean;

  /** Interpolates the position at the given time. */
  getPosition(time: T3Time): number;

  /** Moves all node times by the given offset. */
  nudge(distance: T3Time): void;

  /** Moves all node positions by the given offset. */
  shift(offset: number): void;
}
