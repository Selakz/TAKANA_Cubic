/**
 * The snapshot of a chart, which is a container of some components.
 * Edits of it apply immediately if the chart is from `ctx.loadChart()` or `ctx.createNewChart()`;
 * Edits of it apply only after `ctx.commit()` if the chart is from `ctx.chart`.
 */
declare class ChartSnapshot {
  /** All tracks in the chart. Updates live when tracks are added/removed. */
  readonly tracks: ReadonlySet<TrackSnapshot>;

  /** All notes in the chart. Updates live when notes are added/removed. */
  readonly notes: ReadonlySet<NoteSnapshot>;

  /**
   * BPM list of the chart. Edits here apply immediately
   * (no commit needed) and are NOT undoable.
   */
  readonly bpmList: BpmListWrapper;

  /**
   * Layers of the chart. Edits here apply immediately
   * (no commit needed) and are NOT undoable.
   */
  readonly layersInfo: LayersInfoWrapper;

  readonly offset: T3Time;

  /**
   * Adds a track (and optionally its notes) to the chart.
   */
  addTrack(model: TrackModel, notes?: NoteModel[]): boolean;

  /** Adds a note to the given track instance. */
  addNote(model: NoteModel, track: TrackSnapshot): boolean;

  /**
   * Adds a floating (draft) note that is not attached to any track.
   */
  addDraftNote(model: DraftNoteModel): boolean;

  /**
   * Removes the component from the chart.
   */
  removeComponent(component: ComponentSnapshot): void;
}

/**
 * A Map from time to BPM value, backed by the chart's BPM list.
 * All edits apply immediately (no commit needed) and are not undoable.
 */
declare class BpmListWrapper implements Map<T3Time, number> {
  /**
   * Snaps the time down to the nearest grid line.
   * @param gridDivision: number of grid segments per beat.
   */
  getFloorTime(time: T3Time, gridDivision: number): T3Time;

  /**
   * Snaps the time up to the nearest grid line.
   * @param gridDivision: number of grid segments per beat.
   */
  getCeilTime(time: T3Time, gridDivision: number): T3Time;

  has(key: T3Time): boolean;

  get(key: T3Time): number | undefined;

  /**
   * Deletes the BPM node at the given time.
   * At least one node is always kept (a default 0ms -> 100 is restored when empty).
   */
  delete(key: T3Time): boolean;

  /** Resets the BPM list to a single default node (0ms -> 100). */
  clear(): void;

  keys(): MapIterator<T3Time>;

  values(): MapIterator<number>;

  size: number;

  /** Sets the BPM value at the given time. The value is clamped to at least 1. */
  set(key: T3Time, value: number): this;

  entries(): MapIterator<[T3Time, number]>;

  forEach(
    callbackfn: (value: number, key: T3Time, map: Map<T3Time, number>) => void,
    thisArg?: any,
  ): void;

  [Symbol.iterator](): MapIterator<[T3Time, number]>;

  [Symbol.toStringTag]: string;
}

/** The definition of a track layer. */
interface Layer {
  id: number;
  name: string;
  color: Color;
  isDecoration: boolean;
  isSelected: boolean;
}

/**
 * View of the chart's layers.
 * All edits (`add`/`remove`/`update`) take effect immediately and are NOT
 * undoable (no commit involved).
 */
declare class LayersInfoWrapper {
  /** Snapshots of all layers in order. Re-read on each access. */
  readonly layers: ReadonlyArray<Layer>;

  /** The default layer. It cannot be removed. */
  readonly defaultLayer: Layer;

  /** Adds a new layer; its id is auto-assigned. */
  add(layer: Omit<Layer, "id">): boolean;

  /**
   * Removes a layer by id. Returns false if the layer does not exist or is
   * the default layer. Tracks on the removed layer are reset to the default layer.
   */
  remove(layerId: number): boolean;

  /**
   * Updates name/color/isDecoration/isSelected of a layer by id
   * (its id is kept). Returns false if the layer does not exist.
   */
  update(layerId: number, layer: Omit<Layer, "id">): boolean;
}
