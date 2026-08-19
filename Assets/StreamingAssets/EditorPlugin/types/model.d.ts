/** RGBA color, each channel in [0, 1]. */
interface Color {
  r: number;
  g: number;
  b: number;
  a: number;
}

/** Time value. Stored in milliseconds; `second` is `milli / 1000`. */
declare class T3Time {
  constructor(milli: number);
  readonly milli: number;
  readonly second: number;

  equals(other: T3Time): boolean;
}

/** Multi-language string. */
declare interface I18NString {
  en: string | undefined;
  zh_Hans?: string | undefined;
  zh_Hant?: string | undefined;
  ja?: string | undefined;
}

/**
 * A value holder exposed to the plugin.
 */
interface Wrapper<T> {
  get value(): T;
  set value(val: T);
}

/** Base interface for chart components. */
interface ComponentModel {
  get timeMin(): T3Time;
  get timeMax(): T3Time;

  /** Moves the component by the given time offset. */
  nudge(distance: T3Time): void;
}

/**
 * Snapshot of a component in the current chart.
 * Property writes on its wrappers need `ctx.commit()` to take effect if it's under ctx.chart
 */
interface ComponentSnapshot extends ComponentModel {
  /**
   * The id of the snapshot. It CAN repeat, so don't use it as a unique identifier of the snapshot.
   */
  readonly id: Wrapper<number>;

  readonly name: Wrapper<string | null>;

  /**
   * Returns an independent model copy. Modifying it does NOT affect the chart;
   * inject it back via `ChartSnapshot.addNote` / `addTrack` etc.
   */
  getModel(): ComponentModel;
}

/** Plugin parameters declared in `manifest.json`, keyed by param id. */
declare var params: ReadonlyMap<string, Wrapper<any>>;

/**
 * Class property decorator that binds a property to a plugin param
 * (equivalent to reading/writing `params.get(key).value`).
 */
declare function Param<T>(
  key: string,
): (target: Object, propertyKey: string | symbol) => void;
