import { T3Time } from "../model.js";
import { SetView, toArray } from "./t3chart.js";
import type { ChartSnapshot } from "./t3chart.js";
import { createMoveItem } from "./t3track.js";
import type { MoveItem, TrackSnapshot } from "./t3track.js";
import type { NodeApi, SelectSet } from "./t3context.js";

export interface TrackNode {
  readonly track: TrackSnapshot;
  readonly time: T3Time;

  getNextTime(): T3Time;
  getModel(): MoveItem;
  getRaw(): any;
}

export class TrackEdgeNode implements TrackNode {
  readonly track: TrackSnapshot;

  get isLeft(): boolean {
    return this.raw.isLeft;
  }

  constructor(
    private raw: any,
    private chart: ChartSnapshot,
  ) {
    this.track = this.resolveTrack();
  }

  get time(): T3Time {
    return new T3Time(this.raw.time);
  }

  getNextTime(): T3Time{
    return new T3Time(this.raw.getNextTime());
  }
  
  getModel(): MoveItem {
    return createMoveItem(this.raw.getMoveItem());
  }

  getRaw(): any {
    return this.raw;
  }

  private resolveTrack(): TrackSnapshot {
    const track = this.chart.resolveTrack(this.raw.track);
    if (track === undefined) throw new Error("Track not found");
    return track;
  }
}

export class TrackDirectNode implements TrackNode {
  readonly track: TrackSnapshot;

  get isPosition(): boolean {
    return this.raw.isPosition;
  }

  constructor(
    private raw: any,
    private chart: ChartSnapshot,
  ) {
    this.track = this.resolveTrack();
  }

  get time(): T3Time {
    return new T3Time(this.raw.time);
  }

  getNextTime(): T3Time{
    return new T3Time(this.raw.getNextTime());
  }
  
  getModel(): MoveItem {
    return createMoveItem(this.raw.getMoveItem());
  }

  getRaw(): any {
    return this.raw;
  }

  private resolveTrack(): TrackSnapshot {
    const track = this.chart.resolveTrack(this.raw.track);
    if (track === undefined) throw new Error("Track not found");
    return track;
  }
}

export class NodeDataset implements ReadonlySet<TrackNode> {
  readonly nodes: ReadonlySet<TrackNode>;
  private readonly nodeByRaw: Map<any, TrackNode> = new Map();
  private readonly nodeAddedListeners: ((node: TrackNode) => void)[] = [];
  private readonly nodeRemovedListeners: ((node: TrackNode) => void)[] = [];

  constructor(
    private api: NodeApi,
    private chart: ChartSnapshot,
  ) {
    this.nodes = new SetView(this.nodeByRaw);

    this.api.onNodeAdded((raw: any) => {
      const node = this.createNode(raw);
      this.nodeByRaw.set(raw, node);
      this.fireNodeAdded(node);
    });
    this.api.onNodeRemoved((raw: any) => {
      const node = this.nodeByRaw.get(raw);
      if (node) {
        this.fireNodeRemoved(node);
        this.nodeByRaw.delete(raw);
      }
    });

    for (const raw of toArray(this.api.getAllNodes())) {
      this.nodeByRaw.set(raw, this.createNode(raw));
    }
  }

  get size(): number {
    return this.nodes.size;
  }
  has(value: TrackNode): boolean {
    return this.nodes.has(value);
  }
  forEach(
    callbackfn: (
      value: TrackNode,
      value2: TrackNode,
      set: ReadonlySet<TrackNode>,
    ) => void,
    thisArg?: any,
  ): void {
    this.nodes.forEach(callbackfn, thisArg);
  }
  keys(): IterableIterator<TrackNode> {
    return this.nodes.keys();
  }
  values(): IterableIterator<TrackNode> {
    return this.nodes.values();
  }
  entries(): IterableIterator<[TrackNode, TrackNode]> {
    return this.nodes.entries();
  }
  [Symbol.iterator](): IterableIterator<TrackNode> {
    return this.nodes[Symbol.iterator]();
  }
  get [Symbol.toStringTag](): string {
    return "Set";
  }

  resolveNode(raw: any): TrackNode | undefined {
    return this.nodeByRaw.get(raw);
  }

  _onNodeAdded(listener: (node: TrackNode) => void): void {
    this.nodeAddedListeners.push(listener);
  }

  _onNodeRemoved(listener: (node: TrackNode) => void): void {
    this.nodeRemovedListeners.push(listener);
  }

  private fireNodeAdded(node: TrackNode): void {
    for (const listener of this.nodeAddedListeners) listener(node);
  }

  private fireNodeRemoved(node: TrackNode): void {
    for (const listener of this.nodeRemovedListeners) listener(node);
  }

  private createNode(raw: any): TrackNode {
    if (raw.type === "Edge") return new TrackEdgeNode(raw, this.chart);
    return new TrackDirectNode(raw, this.chart);
  }
}

export class NodeSelectSet implements SelectSet<TrackNode> {
  constructor(
    private api: NodeApi,
    private nodes: NodeDataset,
  ) {}

  get currentSelecting(): TrackNode | undefined {
    const raw = this.api.getCurrentSelecting();
    if (raw === null || raw === undefined) return undefined;
    return this.nodes.resolveNode(raw);
  }

  get size(): number {
    return toArray(this.api.getAllSelected()).length;
  }

  has(value: TrackNode): boolean {
    const raw = value.getRaw();
    for (const selected of toArray(this.api.getAllSelected())) {
      if (selected === raw) return true;
    }
    return false;
  }

  add(value: TrackNode): this {
    this.api.addSelected(value.getRaw());
    return this;
  }

  delete(value: TrackNode): boolean {
    const existed = this.has(value);
    this.api.removeSelected(value.getRaw());
    return existed;
  }

  clear(): void {
    this.api.clearSelected();
  }

  forEach(
    callbackfn: (
      value: TrackNode,
      value2: TrackNode,
      set: Set<TrackNode>,
    ) => void,
    thisArg?: any,
  ): void {
    for (const v of this.values()) callbackfn.call(thisArg, v, v, this);
  }

  keys(): IterableIterator<TrackNode> {
    return this.values();
  }

  *values(): IterableIterator<TrackNode> {
    for (const raw of toArray(this.api.getAllSelected())) {
      const resolved = this.nodes.resolveNode(raw);
      if (resolved !== undefined) yield resolved;
    }
  }

  *entries(): IterableIterator<[TrackNode, TrackNode]> {
    for (const v of this.values()) yield [v, v];
  }

  [Symbol.iterator](): IterableIterator<TrackNode> {
    return this.values();
  }

  get [Symbol.toStringTag](): string {
    return "Set";
  }
}
