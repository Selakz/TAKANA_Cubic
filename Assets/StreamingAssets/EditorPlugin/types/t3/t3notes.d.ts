/**
 * A note as plain data (Model).
 * Modifying it does NOT affect the chart until it is injected via
 * `ChartSnapshot.addNote` / `addDraftNote`.
 */
interface NoteModel extends ComponentModel {
  timeJudge: T3Time;

  /**
   * Whether this note is dummy, which means it doesn't need to be clicked and
   * is not counted into scores when playing this chart.
   * */
  isDummy: boolean;
}

/**
 * Snapshot of a note in the current chart.
 */
interface NoteSnapshot extends ComponentSnapshot {
  readonly timeJudge: Wrapper<T3Time>;

  readonly isDummy: Wrapper<boolean>;
}

declare enum HitType {
  Tap,
  Slide,
}

/** A tap-like note as plain data (Model). */
declare class HitModel implements NoteModel {
  constructor(hitType: HitType, timeJudge: T3Time, isDummy?: boolean);

  hitType: HitType;

  timeJudge: T3Time;

  isDummy: boolean;

  get timeMin(): T3Time;

  get timeMax(): T3Time;

  nudge(distance: T3Time): void;
}

declare class HitSnapshot implements NoteSnapshot {
  readonly id: Wrapper<number>;

  readonly name: Wrapper<string | null>;

  /**
   * The note type.
   * NOTE: changing it deletes and re-creates the note internally, so re-fetch
   * the note from `chart.notes` after commit (old references may be detached).
   */
  readonly hitType: Wrapper<HitType>;

  readonly timeJudge: Wrapper<T3Time>;

  readonly isDummy: Wrapper<boolean>;

  /** The track this note belongs to. */
  readonly track: TrackSnapshot;

  get timeMin(): T3Time;

  get timeMax(): T3Time;

  nudge(distance: T3Time): void;

  /** Returns an independent Model copy; edits don't affect the chart. */
  getModel(): HitModel;
}

/** A hold note as plain data (Model). */
declare class HoldModel implements NoteModel {
  constructor(timeJudge: T3Time, timeEnd: T3Time, isDummy?: boolean);

  timeJudge: T3Time;

  timeEnd: T3Time;

  isDummy: boolean;

  get timeMin(): T3Time;

  get timeMax(): T3Time;

  nudge(distance: T3Time): void;
}

declare class HoldSnapshot implements NoteSnapshot {
  readonly id: Wrapper<number>;

  readonly name: Wrapper<string | null>;

  readonly timeJudge: Wrapper<T3Time>;

  /**
   * End time of the hold.
   * Values are clamped: `timeEnd` is kept greater than `timeJudge` and within
   * the track's time range.
   */
  readonly timeEnd: Wrapper<T3Time>;

  readonly isDummy: Wrapper<boolean>;

  readonly track: TrackSnapshot;

  get timeMin(): T3Time;

  get timeMax(): T3Time;

  nudge(distance: T3Time): void;

  /** Returns an independent Model copy; edits don't affect the chart. */
  getModel(): HoldModel;
}

/**
 * A floating (draft) note that is not attached to any track,
 * with its own horizontal position and width.
 */
interface DraftNoteModel extends NoteModel {
  position: number;

  width: number;
}

/** A draft tap-like note as plain data (Model). */
declare class DraftHitModel extends HitModel implements DraftNoteModel {
  constructor(
    hitType: HitType,
    timeJudge: T3Time,
    position: number,
    width: number,
    isDummy?: boolean,
  );

  position: number;

  width: number;
}

declare class DraftHitSnapshot implements NoteSnapshot {
  readonly id: Wrapper<number>;

  readonly name: Wrapper<string | null>;

  /**
   * The note type.
   * NOTE: changing it deletes and re-creates the note internally, so re-fetch
   * the note from `chart.notes` after commit.
   */
  readonly hitType: Wrapper<HitType>;

  readonly timeJudge: Wrapper<T3Time>;

  readonly position: Wrapper<number>;

  readonly width: Wrapper<number>;

  readonly isDummy: Wrapper<boolean>;

  get timeMin(): T3Time;

  get timeMax(): T3Time;

  nudge(distance: T3Time): void;

  /** Returns an independent Model copy; edits don't affect the chart. */
  getModel(): DraftHitModel;
}

/** A draft hold note as plain data (Model). */
declare class DraftHoldModel extends HoldModel implements DraftNoteModel {
  constructor(
    timeJudge: T3Time,
    timeEnd: T3Time,
    position: number,
    width: number,
    isDummy?: boolean,
  );

  position: number;

  width: number;
}

declare class DraftHoldSnapshot implements NoteSnapshot {
  readonly id: Wrapper<number>;

  readonly name: Wrapper<string | null>;

  readonly timeJudge: Wrapper<T3Time>;

  readonly timeEnd: Wrapper<T3Time>;

  readonly position: Wrapper<number>;

  readonly width: Wrapper<number>;

  readonly isDummy: Wrapper<boolean>;

  get timeMin(): T3Time;

  get timeMax(): T3Time;

  nudge(distance: T3Time): void;

  /** Returns an independent Model copy; edits don't affect the chart. */
  getModel(): DraftHoldModel;
}
