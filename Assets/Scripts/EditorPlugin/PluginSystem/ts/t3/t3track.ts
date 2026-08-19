import {
  T3Time,
  ComponentModel,
  ComponentSnapshot,
  Wrapper,
} from "../model.js";
import type { NoteSnapshot } from "./t3notes.js";
import type { ChartSnapshot, Layer } from "./t3chart.js";

// Eases: mirror of C# T3Framework.Static.Easing.Eases
export enum Eases {
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
Object.freeze(Eases);

export interface MoveItem {
  position: number;
  getPosition(
    thisTime: T3Time,
    targetTime: T3Time,
    nextTime: T3Time,
    nextPosition: number,
  ): number;
  clone(): MoveItem;
  toCSharp(): any;
}

export class EaseMoveItem implements MoveItem {
  constructor(
    public position: number,
    public ease: Eases,
  ) {}

  getPosition(
    thisTime: T3Time,
    targetTime: T3Time,
    nextTime: T3Time,
    nextPosition: number,
  ): number {
    const t =
      (targetTime.second - thisTime.second) /
      (nextTime.second - thisTime.second);
    const opposite = CS.EditorPlugin.Shared.To.RawMoveItem.opposite(this.ease);
    return CS.EditorPlugin.Shared.To.RawMoveItem.calcCoord(
      opposite,
      this.position,
      nextPosition,
      t,
    );
  }

  clone(): EaseMoveItem {
    return new EaseMoveItem(this.position, this.ease);
  }

  toCSharp(): any {
    return new CS.MusicGame.Models.Track.Movement.V1EMoveItem(
      this.position,
      this.ease,
    );
  }
}

export class BezierMoveItem implements MoveItem {
  constructor(
    public position: number,
    public startTimeFactor: number,
    public startPositionFactor: number,
    public endTimeFactor: number,
    public endPositionFactor: number,
  ) {}

  getPosition(
    thisTime: T3Time,
    targetTime: T3Time,
    nextTime: T3Time,
    nextPosition: number,
  ): number {
    if (thisTime.milli === nextTime.milli) return nextPosition;
    const iterationTimes = 5;
    const timeT =
      (targetTime.second - thisTime.second) /
      (nextTime.second - thisTime.second);

    let factorT = timeT;
    for (let i = 0; i < iterationTimes; i++) {
      const currentT = cubicBezier(
        0,
        this.startTimeFactor,
        this.endTimeFactor,
        1,
        factorT,
      );
      const slope = cubicDerivative(
        0,
        this.startTimeFactor,
        this.endTimeFactor,
        1,
        factorT,
      );
      if (Math.abs(slope) < 1e-6) break;

      factorT -= (currentT - timeT) / slope;
      factorT = Math.max(0, Math.min(1, factorT));
    }

    const positionT = cubicBezier(
      0,
      this.startPositionFactor,
      this.endPositionFactor,
      1,
      factorT,
    );
    return this.position + (nextPosition - this.position) * positionT;
  }

  clone(): BezierMoveItem {
    return new BezierMoveItem(
      this.position,
      this.startTimeFactor,
      this.startPositionFactor,
      this.endTimeFactor,
      this.endPositionFactor,
    );
  }

  toCSharp(): any {
    return new CS.MusicGame.Models.Track.Movement.V1BMoveItem(
      this.position,
      new CS.UnityEngine.Vector2(this.startTimeFactor, this.startPositionFactor),
      new CS.UnityEngine.Vector2(this.endTimeFactor, this.endPositionFactor),
    );
  }
}

function cubicBezier(
  start: number,
  startControl: number,
  endControl: number,
  end: number,
  t: number,
): number {
  const u = 1 - t;
  return (
    u * u * u * start +
    3 * u * u * t * startControl +
    3 * u * t * t * endControl +
    t * t * t * end
  );
}

function cubicDerivative(
  start: number,
  startControl: number,
  endControl: number,
  end: number,
  t: number,
): number {
  const u = 1 - t;
  return (
    3 * u * u * (startControl - start) +
    6 * u * t * (endControl - startControl) +
    3 * t * t * (end - endControl)
  );
}

export class MoveList {
  readonly items: Map<number, MoveItem> = new Map();
  
  constructor(items?: ReadonlyMap<T3Time, MoveItem>) {
    if (items === undefined) return;
    for (const [time, item] of items) {
      this.items.set(time.milli, item);
    }
  }

  set(time: T3Time, item: MoveItem): boolean {
    this.items.set(time.milli, item);
    return true;
  }

  delete(time: T3Time): boolean {
    return this.items.delete(time.milli);
  }

  getPosition(time: T3Time): number {
    const times = [...this.items.keys()].sort((a, b) => a - b);
    if (times.length === 0) return 0;
    const first = times[0];
    if (time.milli <= first) return this.items.get(first)!.position;
    const last = times[times.length - 1];
    if (time.milli >= last) return this.items.get(last)!.position;

    let prev = first;
    for (const key of times) {
      if (key > time.milli) break;
      prev = key;
    }
    let next = first;
    for (const key of times) {
      if (key > prev) {
        next = key;
        break;
      }
    }
    return this.items
      .get(prev)!
      .getPosition(new T3Time(prev), time, new T3Time(next), this.items.get(next)!.position);
  }

  nudge(distance: T3Time): void {
    const entries = [...this.items.entries()];
    this.items.clear();
    for (const [time, item] of entries) {
      this.items.set(time + distance.milli, item);
    }
  }

  shift(offset: number): void {
    for (const item of this.items.values()) item.position += offset;
  }

  toCSharp(): any {
    const list = CS.EditorPlugin.Shared.To.RawTrackData.NewMoveList();
    for (const [time, item] of this.items) {
      CS.EditorPlugin.Shared.To.RawTrackData.Insert(list, new CS.T3Framework.Runtime.T3Time(time), item.toCSharp());
    }
    return list;
  }
}

export interface TrackMovement {
  getPosition(time: T3Time): number;
  getWidth(time: T3Time): number;
  getLeftPosition(time: T3Time): number;
  getRightPosition(time: T3Time): number;
  nudge(distance: T3Time): void;
  shift(offset: number): void;
  insert(time: T3Time, position: number, width: number): void;
}

export class TrackEdgeMovement implements TrackMovement {
  constructor(
    public leftMoveList: MoveList,
    public rightMoveList: MoveList,
  ) {}

  getPosition(time: T3Time): number {
    return (
      (this.leftMoveList.getPosition(time) +
        this.rightMoveList.getPosition(time)) /
      2
    );
  }
  getWidth(time: T3Time): number {
    return Math.abs(
      this.leftMoveList.getPosition(time) -
        this.rightMoveList.getPosition(time),
    );
  }
  getLeftPosition(time: T3Time): number {
    return this.leftMoveList.getPosition(time);
  }
  getRightPosition(time: T3Time): number {
    return this.rightMoveList.getPosition(time);
  }
  nudge(distance: T3Time): void {
    this.leftMoveList.nudge(distance);
    this.rightMoveList.nudge(distance);
  }
  shift(offset: number): void {
    this.leftMoveList.shift(offset);
    this.rightMoveList.shift(offset);
  }
  insert(time: T3Time, position: number, width: number): void {
    this.leftMoveList.set(
      time,
      new EaseMoveItem(position - width / 2, Eases.Unmove),
    );
    this.rightMoveList.set(
      time,
      new EaseMoveItem(position + width / 2, Eases.Unmove),
    );
  }

  toCSharp(): any {
    return new CS.MusicGame.Models.Track.Movement.TrackEdgeMovement(
      this.leftMoveList.toCSharp(),
      this.rightMoveList.toCSharp(),
    );
  }
}

export class TrackDirectMovement implements TrackMovement {
  constructor(
    public positionMoveList: MoveList,
    public widthMoveList: MoveList,
  ) {}

  getPosition(time: T3Time): number {
    return this.positionMoveList.getPosition(time);
  }
  getWidth(time: T3Time): number {
    return this.widthMoveList.getPosition(time);
  }
  getLeftPosition(time: T3Time): number {
    return this.getPosition(time) - this.getWidth(time) / 2;
  }
  getRightPosition(time: T3Time): number {
    return this.getPosition(time) + this.getWidth(time) / 2;
  }
  nudge(distance: T3Time): void {
    this.positionMoveList.nudge(distance);
    this.widthMoveList.nudge(distance);
  }
  shift(offset: number): void {
    this.positionMoveList.shift(offset);
    this.widthMoveList.shift(offset);
  }
  insert(time: T3Time, position: number, width: number): void {
    this.positionMoveList.set(time, new EaseMoveItem(position, Eases.Unmove));
    this.widthMoveList.set(time, new EaseMoveItem(width, Eases.Unmove));
  }

  toCSharp(): any {
    return new CS.MusicGame.Models.Track.Movement.TrackDirectMovement(
      this.positionMoveList.toCSharp(),
      this.widthMoveList.toCSharp(),
    );
  }
}

export class TrackModel implements ComponentModel {
  constructor(
    public timeStart: T3Time,
    public timeEnd: T3Time,
    public movement: TrackMovement,
  ) {}

  get timeMin(): T3Time {
    return this.timeStart;
  }
  get timeMax(): T3Time {
    return this.timeEnd;
  }
  nudge(distance: T3Time): void {
    this.timeStart = new T3Time(this.timeStart.milli + distance.milli);
    this.timeEnd = new T3Time(this.timeEnd.milli + distance.milli);
    this.movement.nudge(distance);
  }
  shift(offset: number): void {
    this.movement.shift(offset);
  }

  toCSharp(): any {
    const track = new CS.MusicGame.Models.Track.Track(
      new CS.T3Framework.Runtime.T3Time(this.timeStart.milli),
      new CS.T3Framework.Runtime.T3Time(this.timeEnd.milli),
    );
    track.Movement = (this.movement as any).toCSharp();
    return track;
  }
}

export interface TrackMovementWrapper {
  getPosition(time: T3Time): number;
  getWidth(time: T3Time): number;
  getLeftPosition(time: T3Time): number;
  getRightPosition(time: T3Time): number;
  insert(time: T3Time, position: number, width: number): void;
  getModel(): TrackMovement;
}

export function createMoveItem(raw: any): MoveItem {
  if (raw.type === "ease") return new EaseMoveItem(raw.position, raw.ease);
  return new BezierMoveItem(
    raw.position,
    raw.startTimeFactor,
    raw.startPositionFactor,
    raw.endTimeFactor,
    raw.endPositionFactor,
  );
}

function createMoveList(raw: any, flag: boolean): MoveList {
  const list = new MoveList();
  const items = raw.getItems(flag);
  for (let i = 0; i < items.Length; i++) {
    const itemRaw = items.get_Item(i);
    list.set(new T3Time(itemRaw.time), createMoveItem(itemRaw));
  }
  return list;
}

export class TrackEdgeMovementWrapper implements TrackMovementWrapper {
  constructor(private raw: any) {}

  getPosition(time: T3Time): number {
    return this.raw.getPosition(time.milli);
  }
  getWidth(time: T3Time): number {
    return this.raw.getWidth(time.milli);
  }
  getLeftPosition(time: T3Time): number {
    return this.raw.getLeftPosition(time.milli);
  }
  getRightPosition(time: T3Time): number {
    return this.raw.getRightPosition(time.milli);
  }

  get(time: T3Time, isLeft: boolean): MoveItem | undefined {
    const raw = this.raw.getItem(time.milli, isLeft);
    return raw === null || raw === undefined ? undefined : createMoveItem(raw);
  }

  getModel(): TrackEdgeMovement {
    return new TrackEdgeMovement(
      createMoveList(this.raw, true),
      createMoveList(this.raw, false),
    );
  }

  set(time: T3Time, item: MoveItem, isLeft: boolean): boolean {
    return this.raw.set(time.milli, item.toCSharp(), isLeft);
  }
  delete(time: T3Time, isLeft: boolean): boolean {
    return this.raw.delete(time.milli, isLeft);
  }
  insert(time: T3Time, position: number, width: number): void {
    this.raw.insert(time.milli, position, width);
  }
}

export class TrackDirectMovementWrapper implements TrackMovementWrapper {
  constructor(private raw: any) {}

  getPosition(time: T3Time): number {
    return this.raw.getPosition(time.milli);
  }
  getWidth(time: T3Time): number {
    return this.raw.getWidth(time.milli);
  }
  getLeftPosition(time: T3Time): number {
    return this.raw.getLeftPosition(time.milli);
  }
  getRightPosition(time: T3Time): number {
    return this.raw.getRightPosition(time.milli);
  }

  get(time: T3Time, isPosition: boolean): MoveItem | undefined {
    const raw = this.raw.getItem(time.milli, isPosition);
    return raw === null || raw === undefined ? undefined : createMoveItem(raw);
  }

  getModel(): TrackDirectMovement {
    return new TrackDirectMovement(
      createMoveList(this.raw, true),
      createMoveList(this.raw, false),
    );
  }

  set(time: T3Time, item: MoveItem, isPosition: boolean): boolean {
    return this.raw.set(time.milli, item.toCSharp(), isPosition);
  }
  delete(time: T3Time, isPosition: boolean): boolean {
    return this.raw.delete(time.milli, isPosition);
  }
  insert(time: T3Time, position: number, width: number): void {
    this.raw.insert(time.milli, position, width);
  }
}

export class TrackSnapshot implements ComponentSnapshot {
  readonly id: Wrapper<number>;
  readonly name: Wrapper<string>;
  readonly movement: TrackMovementWrapper;

  constructor(
    private raw: any,
    private chart: ChartSnapshot,
  ) {
    this.id = raw.id;
    this.name = raw.name;
    this.movement =
      raw.movement.type === "Edge"
        ? new TrackEdgeMovementWrapper(raw.movement)
        : new TrackDirectMovementWrapper(raw.movement);
  }

  get notes(): Iterable<NoteSnapshot> {
    return this.getNotes();
  }

  private *getNotes(): IterableIterator<NoteSnapshot> {
    for (const note of this.chart.notes) {
      if (note.track === this) yield note;
    }
  }

  getRaw(): any {
    return this.raw;
  }

  get timeMin(): T3Time {
    return new T3Time(this.raw.timeStart.value);
  }
  get timeMax(): T3Time {
    return new T3Time(this.raw.timeEnd.value);
  }

  get layer(): Layer {
    const layer = this.raw.getLayer();
    return {
      id: layer.id,
      name: layer.name,
      color: {
        r: layer.color.r,
        g: layer.color.g,
        b: layer.color.b,
        a: layer.color.a,
      },
      isDecoration: layer.isDecoration,
      isSelected: layer.isSelected,
    };
  }
  
  setLayer(id: number): void {
    this.raw.setLayer(id);
  }

  nudge(distance: T3Time): void {
    this.raw.nudge(distance.milli);
  }
  shift(offset: number): void {
    this.raw.shift(offset);
  }

  getModel(): TrackModel {
    return new TrackModel(
      new T3Time(this.raw.timeStart.value),
      new T3Time(this.raw.timeEnd.value),
      this.movement.getModel(),
    );
  }
}
