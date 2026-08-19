export interface Wrapper<T> {
  value: T;
}

export class EmptyWrapper<T> implements Wrapper<T> {
  value: T;
  constructor(val: T) {
    this.value = val;
  }
}

export class T3Time {
  constructor(private _milli: number) {}
  get milli(): number {
    return this._milli;
  }
  get second(): number {
    return this._milli / 1000;
  }
  equals(other: T3Time): boolean {
    return other != null && this._milli === other._milli;
  }
}

export class T3TimeWrapper implements Wrapper<T3Time> {
  constructor(private inner: Wrapper<number>) {}

  get value(): T3Time {
    return new T3Time(this.inner.value);
  }
  set value(v: T3Time) {
    this.inner.value = v.milli;
  }
}

export interface I18NString {
  en: string | undefined;
  zh_Hans: string | undefined;
  zh_Hant: string | undefined;
  ja: string | undefined;
}

export interface ComponentModel {
  readonly timeMin: T3Time;
  readonly timeMax: T3Time;
  nudge(distance: T3Time): void;
}

export interface ComponentSnapshot extends ComponentModel {
  readonly id: Wrapper<number>;
  readonly name: Wrapper<string>;
  getModel(): ComponentModel;
  getRaw(): any;
}

export var params: ReadonlyMap<string, Wrapper<any>> = new Map();

export function __params_init(ids: string[], wrappers: Wrapper<any>[]): void {
  const map = new Map<string, Wrapper<any>>();
  // @ts-expect-error
  for (let i = 0; i < ids.Length; i++) {
    // @ts-expect-error
    const raw = wrappers.get_Item(i);
    // @ts-expect-error
    map.set(ids.get_Item(i), {
      get value(): any { return raw.value; },
      // PuerTS ExpressionWrap wraps number to int if corresponding C# type is object; so if not stringify, float value will be truncated.
      set value(v: any) { raw.value = typeof v === "number" ? String(v) : v; },
    });
  }
  params = map;
  globalThis.params = params;
}

export function Param<T>(key: string) {
  return function(target: any, propertyKey: string) {
    Object.defineProperty(target, propertyKey, {
      get: function(): T {
        return params.get(key)!.value as T;
      },
      set: function(value: T) {
        params.get(key)!.value = value;
      },
      enumerable: true,
      configurable: true
    });
  };
}