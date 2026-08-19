import {
  NoteSnapshot,
  NoteModel,
  DraftNoteModel,
  HitSnapshot,
  HoldSnapshot,
  DraftHitSnapshot,
  DraftHoldSnapshot,
} from "./t3notes.js";
import { TrackSnapshot, TrackModel } from "./t3track.js";
import type { ChartApi, SelectSet } from "./t3context.js";
import { ComponentSnapshot, T3Time } from "../model.js";

export function toArray(arr: any): any[] {
  if (Array.isArray(arr)) return arr;
  const result: any[] = [];
  for (let i = 0; i < arr.Length; i++) {
    result.push(arr.get_Item(i));
  }
  return result;
}

export class BpmListWrapper implements Map<T3Time, number> {
  constructor(private raw: any) {}

  getFloorTime(time: T3Time, gridDivision: number): T3Time {
    return new T3Time(this.raw.getFloorTime(time.milli, gridDivision));
  }

  getCeilTime(time: T3Time, gridDivision: number): T3Time {
    return new T3Time(this.raw.getCeilTime(time.milli, gridDivision));
  }

  has(key: T3Time): boolean {
    return this.raw.has(key.milli);
  }

  get(key: T3Time): number | undefined {
    const value = this.raw.get(key.milli);
    return value === null || value === undefined ? undefined : value;
  }

  delete(key: T3Time): boolean {
    return this.raw.delete(key.milli);
  }

  clear(): void {
    this.raw.clear();
  }

  get size(): number {
    return this.raw.size;
  }

  set(key: T3Time, value: number): this {
    this.raw.set(key.milli, value);
    return this;
  }

  *keys(): IterableIterator<T3Time> {
    for (const milli of toArray(this.raw.keys())) {
      yield new T3Time(milli);
    }
  }

  *values(): IterableIterator<number> {
    for (const value of toArray(this.raw.values())) {
      yield value;
    }
  }

  *entries(): IterableIterator<[T3Time, number]> {
    for (const milli of toArray(this.raw.keys())) {
      yield [new T3Time(milli), this.get(new T3Time(milli))!];
    }
  }

  forEach(
    callbackfn: (value: number, key: T3Time, map: Map<T3Time, number>) => void,
    thisArg?: any,
  ): void {
    for (const [key, value] of this.entries()) {
      callbackfn.call(thisArg, value, key, this);
    }
  }

  *[Symbol.iterator](): IterableIterator<[T3Time, number]> {
    yield* this.entries();
  }

  get [Symbol.toStringTag](): string {
    return "Map";
  }
}

export class LayersInfoWrapper {
  constructor(private raw: any) {}

  get layers(): Layer[] {
    return toArray(this.raw.layers);
  }

  get defaultLayer(): Layer {
    return this.raw.defaultLayer;
  }

  add(layer: Omit<Layer, "id">): boolean {
    return this.raw.add(toCSharpLayer(layer));
  }

  remove(layerId: number): boolean {
    return this.raw.remove(layerId);
  }

  update(layerId: number, layer: Omit<Layer, "id">): boolean {
    return this.raw.update(layerId, toCSharpLayer(layer));
  }
}

function toCSharpLayer(layer: Omit<Layer, "id">): any {
  const info = new CS.MusicGame.ChartEditor.TrackLayer.LayerInfo();
  info.Name = layer.name;
  info.Color = new CS.UnityEngine.Color(
    layer.color.r,
    layer.color.g,
    layer.color.b,
    layer.color.a,
  );
  info.IsDecoration = layer.isDecoration;
  info.IsSelected = layer.isSelected;
  return info;
}

export class SetView<T> implements ReadonlySet<T> {
  constructor(private map: Map<any, T>) {}

  get size(): number {
    return this.map.size;
  }
  has(value: T): boolean {
    for (const v of this.map.values()) {
      if (v === value) return true;
    }
    return false;
  }
  forEach(
    callbackfn: (value: T, value2: T, set: ReadonlySet<T>) => void,
    thisArg?: any,
  ): void {
    for (const v of this.map.values()) callbackfn.call(thisArg, v, v, this);
  }
  keys(): IterableIterator<T> {
    return this.map.values();
  }
  values(): IterableIterator<T> {
    return this.map.values();
  }
  *entries(): IterableIterator<[T, T]> {
    for (const v of this.map.values()) yield [v, v];
  }
  [Symbol.iterator](): IterableIterator<T> {
    return this.map.values();
  }
  get [Symbol.toStringTag](): string {
    return "Set";
  }
}

export class ChartSnapshot {
  readonly notes: ReadonlySet<NoteSnapshot>;
  readonly tracks: ReadonlySet<TrackSnapshot>;
  readonly bpmList: BpmListWrapper;
  readonly layersInfo: LayersInfoWrapper;

  private readonly noteByRaw: Map<any, NoteSnapshot> = new Map();
  private readonly trackByRaw: Map<any, TrackSnapshot> = new Map();
  private readonly noteAddedListeners: ((note: NoteSnapshot) => void)[] = [];
  private readonly noteRemovedListeners: ((note: NoteSnapshot) => void)[] = [];
  private readonly trackAddedListeners: ((track: TrackSnapshot) => void)[] = [];
  private readonly trackRemovedListeners: ((track: TrackSnapshot) => void)[] = [];

  constructor(private chartApi: ChartApi) {
    this.notes = new SetView(this.noteByRaw);
    this.tracks = new SetView(this.trackByRaw);
    this.bpmList = new BpmListWrapper(this.chartApi.bpmList);
    this.layersInfo = new LayersInfoWrapper(this.chartApi.layersInfo);

    this.chartApi.onNoteAdded((raw: any) => {
      const note = this.createNote(raw);
      this.noteByRaw.set(raw, note);
      this.fireNoteAdded(note);
    });
    this.chartApi.onNoteRemoved((raw: any) => {
      const note = this.noteByRaw.get(raw);
      if (note) {
        this.fireNoteRemoved(note);
        this.noteByRaw.delete(raw);
      }
    });
    this.chartApi.onTrackAdded((raw: any) => {
      const track = this.createTrack(raw);
      this.trackByRaw.set(raw, track);
      this.fireTrackAdded(track);
    });
    this.chartApi.onTrackRemoved((raw: any) => {
      const track = this.trackByRaw.get(raw);
      if (track) {
        this.fireTrackRemoved(track);
        this.trackByRaw.delete(raw);
      }
    });

    const initialNotes = this.chartApi.getAllNotes();
    // @ts-expect-error
    for (let i = 0; i < initialNotes.Length; i++) {
      // @ts-expect-error
      const raw = initialNotes.get_Item(i);
      this.noteByRaw.set(raw, this.createNote(raw));
    }
    const initialTracks = this.chartApi.getAllTracks();
    // @ts-expect-error
    for (let i = 0; i < initialTracks.Length; i++) {
      // @ts-expect-error
      const raw = initialTracks.get_Item(i);
      this.trackByRaw.set(raw, this.createTrack(raw));
    }
  }

  get offset(): T3Time {
    return new T3Time(this.chartApi.offsetMilli);
  }

  getChartApi(): any {
    return this.chartApi;
  }

  addTrack(model: TrackModel, notes: NoteModel[] = []): boolean {
    // @ts-expect-error
    let arr = CS.System.Array.CreateInstance(puer.$typeof(CS.System.Object), notes.length);
    for (let i = 0; i < notes.length; i++) {
      arr.set_Item(i, notes[i].toCSharp());
    }
    this.chartApi.addTrack(model.toCSharp(), arr);
    return true;
  }

  addNote(model: NoteModel, track: TrackSnapshot): boolean {
    this.chartApi.addNote(model.toCSharp(), track.getRaw());
    return true;
  }

  addDraftNote(model: DraftNoteModel): boolean {
    this.chartApi.addDraftNote(model.toCSharp());
    return true;
  }

  removeComponent(component: ComponentSnapshot): void {
    this.chartApi.removeComponent(component.getRaw());
  }

  resolveNote(raw: any): NoteSnapshot | undefined {
    return this.noteByRaw.get(raw);
  }

  resolveTrack(raw: any): TrackSnapshot | undefined {
    return this.trackByRaw.get(raw);
  }

  _onNoteAdded(listener: (note: NoteSnapshot) => void): void {
    this.noteAddedListeners.push(listener);
  }

  _onNoteRemoved(listener: (note: NoteSnapshot) => void): void {
    this.noteRemovedListeners.push(listener);
  }

  _onTrackAdded(listener: (track: TrackSnapshot) => void): void {
    this.trackAddedListeners.push(listener);
  }

  _onTrackRemoved(listener: (track: TrackSnapshot) => void): void {
    this.trackRemovedListeners.push(listener);
  }

  private fireNoteAdded(note: NoteSnapshot): void {
    for (const listener of this.noteAddedListeners) listener(note);
  }

  private fireNoteRemoved(note: NoteSnapshot): void {
    for (const listener of this.noteRemovedListeners) listener(note);
  }

  private fireTrackAdded(track: TrackSnapshot): void {
    for (const listener of this.trackAddedListeners) listener(track);
  }

  private fireTrackRemoved(track: TrackSnapshot): void {
    for (const listener of this.trackRemovedListeners) listener(track);
  }

  private createNote(raw: any): NoteSnapshot {
    switch (raw.type) {
      case "Hit":
        return new HitSnapshot(raw, this);
      case "Hold":
        return new HoldSnapshot(raw, this);
      case "DraftHit":
        return new DraftHitSnapshot(raw, this);
      case "DraftHold":
        return new DraftHoldSnapshot(raw, this);
      default:
        return new HoldSnapshot(raw, this);
    }
  }

  private createTrack(raw: any): TrackSnapshot {
    return new TrackSnapshot(raw, this);
  }

  // TODO: addNote、addTrack
}

export class ChartSelectSet implements SelectSet<ComponentSnapshot> {
  constructor(
    private api: ChartApi,
    private chart: ChartSnapshot,
  ) {}

  get currentSelecting(): ComponentSnapshot | undefined {
    const raw = this.api.getCurrentSelecting();
    if (raw === null || raw === undefined) return undefined;
    return this.resolve(raw);
  }

  get size(): number {
    return toArray(this.api.getAllSelected()).length;
  }

  has(value: ComponentSnapshot): boolean {
    const raw = value.getRaw();
    for (const selected of toArray(this.api.getAllSelected())) {
      if (selected === raw) return true;
    }
    return false;
  }

  add(value: ComponentSnapshot): this {
    this.api.addSelected(value.getRaw());
    return this;
  }

  delete(value: ComponentSnapshot): boolean {
    const existed = this.has(value);
    this.api.removeSelected(value.getRaw());
    return existed;
  }

  clear(): void {
    this.api.clearSelected();
  }

  forEach(
    callbackfn: (
      value: ComponentSnapshot,
      value2: ComponentSnapshot,
      set: Set<ComponentSnapshot>,
    ) => void,
    thisArg?: any,
  ): void {
    for (const v of this.values()) callbackfn.call(thisArg, v, v, this);
  }

  keys(): IterableIterator<ComponentSnapshot> {
    return this.values();
  }

  *values(): IterableIterator<ComponentSnapshot> {
    for (const raw of toArray(this.api.getAllSelected())) {
      const resolved = this.resolve(raw);
      if (resolved !== undefined) yield resolved;
    }
  }

  *entries(): IterableIterator<[ComponentSnapshot, ComponentSnapshot]> {
    for (const v of this.values()) yield [v, v];
  }

  [Symbol.iterator](): IterableIterator<ComponentSnapshot> {
    return this.values();
  }

  get [Symbol.toStringTag](): string {
    return "Set";
  }

  private resolve(raw: any): ComponentSnapshot | undefined {
    return this.chart.resolveNote(raw) ?? this.chart.resolveTrack(raw);
  }
}

export interface Layer {
  id: number;
  name: string;
  color: { r: number; g: number; b: number; a: number };
  isDecoration: boolean;
  isSelected: boolean;
}
