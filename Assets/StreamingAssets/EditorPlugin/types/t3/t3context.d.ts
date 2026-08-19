/** Log severity used by `ctx.showHeader`. */
declare enum LogType {
  Info,
  Success,
  Warn,
  Error,
}

/**
 * Mouse state retrieval.
 * All methods return `undefined` when the mouse is not on a valid editor area.
 */
interface MouseInfoRetriever {
  /** The start time of a note if it is created by mouse. */
  getTimeStart(): T3Time | undefined;

  /** The end time of a hold if it is created by mouse. */
  getHoldTimeEnd(): T3Time | undefined;

  /** The end time of a track if it is created by mouse. */
  getTrackTimeEnd(): T3Time | undefined;

  /** The width of a track if it is created by mouse. */
  getWidth(): number | undefined;

  /** The position of a track if it is created by mouse. */
  getPosition(): number | undefined;

  /** The position of the nearest vertical grid of mouse (or the exact position of mouse if vertical grids are not on). */
  getAttachedPosition(): number | undefined;
}

/** A Set of selectable objects that also exposes the current selection. */
interface SelectSet<T> extends Set<T> {
  /** The object currently hovered/selected, if any. */
  readonly currentSelecting: T | undefined;
}

/** The plugin's entry context. */
interface T3Context {
  /**
   * The chart currently being edited.
   * NOTE: modifications to it only take effect after calling `commit()`.
   */
  readonly chart: ChartSnapshot;

  /**
   * Selection set of chart components.
   * Selection changes apply immediately (no commit needed).
   */
  readonly chartSelectDataset: SelectSet<ComponentSnapshot>;

  /**
   * Current playback/editing time.
   */
  readonly chartTime: Wrapper<T3Time>;

  /** Total audio length of the music. */
  readonly audioLength: T3Time;

  /** All track movement nodes (live view). */
  readonly nodes: ReadonlySet<TrackNode>;

  /**
   * Selection set of track nodes. Selection changes apply immediately.
   * NOTE: Nodes are added when a track is selected, and are removed when a track is deselected, which means
   * this set is empty when no track is selected.
   */
  readonly nodeSelectDataset: SelectSet<TrackNode>;

  readonly mouseInfoRetriever: MouseInfoRetriever;

  /** Shows a temporal message poped up from the top of the editor. */
  showHeader(content: I18NString, logType: LogType): void;

  /** Shows a confirm dialog; the callback runs when confirmed. */
  showConfirm(content: I18NString, callback: () => void): void;

  /** Shows a confirm/cancel dialog; the callback receives the button index (0 = confirm, 1 = cancel). */
  showConfirmAndCancel(
    content: I18NString,
    callback: (choice: number) => void,
  ): void;

  /**
   * Loads an independent chart from a file. The relative path is relative to
   * the plugin directory.
   * Edits on the returned chart apply immediately and are NOT undoable.
   */
  loadChart(path: string): ChartSnapshot | undefined;

  /**
   * Creates an empty independent chart.
   * Edits on it apply immediately and are NOT undoable.
   */
  createNewChart(): ChartSnapshot;

  /**
   * Saves a chart to a file. The relative path is relative to the plugin
   * directory. Returns whether the save succeeded.
   */
  saveChart(path: string, chart: ChartSnapshot): boolean;

  /**
   * Commits all staged edits on the main chart (wrapper writes, add/remove
   * components, etc.), making them effective and undoable.
   */
  commit(): void;
}

/** Returns the plugin context. */
declare function getT3Context(): T3Context;
