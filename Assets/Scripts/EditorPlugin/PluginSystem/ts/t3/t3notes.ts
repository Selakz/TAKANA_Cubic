import {
  Wrapper,
  T3Time,
  T3TimeWrapper,
  ComponentModel,
  ComponentSnapshot,
} from "../model.js";
import type { ChartSnapshot } from "./t3chart.js";
import type { TrackSnapshot } from "./t3track.js";

export enum HitType {
  Tap,
  Slide,
}

export interface NoteModel extends ComponentModel {
  timeJudge: T3Time;
  isDummy: boolean;
  toCSharp(): any;
}

export interface NoteSnapshot extends ComponentSnapshot {
  readonly timeJudge: Wrapper<T3Time>;
  readonly isDummy: Wrapper<boolean>;
  readonly track?: TrackSnapshot;
  getRaw(): any;
}

interface RawHitData {
  type: string;
  id: Wrapper<number>;
  name: Wrapper<string>;
  hitType: Wrapper<HitType>;
  timeJudge: Wrapper<number>;
  isDummy: Wrapper<boolean>;
  track: any;
}

interface RawHoldData {
  type: string;
  id: Wrapper<number>;
  name: Wrapper<string>;
  timeJudge: Wrapper<number>;
  timeEnd: Wrapper<number>;
  isDummy: Wrapper<boolean>;
  track: any;
}

export class HitModel implements NoteModel {
  constructor(
    private _hitType: HitType,
    private _timeJudge: T3Time,
    private _isDummy: boolean = false,
  ) {}

  get hitType(): HitType {
    return this._hitType;
  }
  set hitType(v: HitType) {
    this._hitType = v;
  }
  get timeJudge(): T3Time {
    return this._timeJudge;
  }
  set timeJudge(v: T3Time) {
    this._timeJudge = v;
  }
  get isDummy(): boolean {
    return this._isDummy;
  }
  set isDummy(v: boolean) {
    this._isDummy = v;
  }
  get timeMin(): T3Time {
    return this._timeJudge;
  }
  get timeMax(): T3Time {
    return this._timeJudge;
  }
  nudge(distance: T3Time): void {
    this._timeJudge = new T3Time(this._timeJudge.milli + distance.milli);
  }

  toCSharp(): any {
    return new CS.MusicGame.Models.Note.Hit(
      new CS.T3Framework.Runtime.T3Time(this.timeJudge.milli),
      this.hitType,
    );
  }
}

export class HitSnapshot implements NoteSnapshot {
  readonly id: Wrapper<number>;
  readonly name: Wrapper<string>;
  readonly hitType: Wrapper<HitType>;
  readonly timeJudge: T3TimeWrapper;
  readonly isDummy: Wrapper<boolean>;

  constructor(
    private raw: RawHitData,
    private chart: ChartSnapshot,
  ) {
    this.id = raw.id;
    this.name = raw.name;
    this.hitType = raw.hitType;
    this.timeJudge = new T3TimeWrapper(raw.timeJudge);
    this.isDummy = raw.isDummy;
  }

  get track(): TrackSnapshot {
    const track = this.chart.resolveTrack(this.raw.track);
    if (track === undefined) throw new Error("Track not found");
    return track;
  }

  getRaw(): any {
    return this.raw;
  }

  get timeMin(): T3Time {
    return new T3Time(this.raw.timeJudge.value);
  }
  get timeMax(): T3Time {
    return new T3Time(this.raw.timeJudge.value);
  }
  nudge(distance: T3Time): void {
    this.timeJudge.value = new T3Time(
      this.timeJudge.value.milli + distance.milli,
    );
  }
  getModel(): HitModel {
    return new HitModel(
      this.raw.hitType.value,
      new T3Time(this.raw.timeJudge.value),
      this.raw.isDummy.value,
    );
  }
}

export class HoldModel implements NoteModel {
  constructor(
    private _timeJudge: T3Time,
    private _timeEnd: T3Time,
    private _isDummy: boolean = false,
  ) {}

  get timeJudge(): T3Time {
    return this._timeJudge;
  }
  set timeJudge(v: T3Time) {
    this._timeJudge = v;
  }
  get timeEnd(): T3Time {
    return this._timeEnd;
  }
  set timeEnd(v: T3Time) {
    this._timeEnd = v;
  }
  get isDummy(): boolean {
    return this._isDummy;
  }
  set isDummy(v: boolean) {
    this._isDummy = v;
  }
  get timeMin(): T3Time {
    return this._timeJudge;
  }
  get timeMax(): T3Time {
    return this._timeEnd;
  }
  nudge(distance: T3Time): void {
    this._timeJudge = new T3Time(this._timeJudge.milli + distance.milli);
    this._timeEnd = new T3Time(this._timeEnd.milli + distance.milli);
  }

  toCSharp(): any {
    return new CS.MusicGame.Models.Note.Hold(
      new CS.T3Framework.Runtime.T3Time(this.timeJudge.milli),
      new CS.T3Framework.Runtime.T3Time(this.timeEnd.milli),
    );
  }
}

export class HoldSnapshot implements NoteSnapshot {
  readonly id: Wrapper<number>;
  readonly name: Wrapper<string>;
  readonly timeJudge: T3TimeWrapper;
  readonly timeEnd: T3TimeWrapper;
  readonly isDummy: Wrapper<boolean>;

  constructor(
    private raw: RawHoldData,
    private chart: ChartSnapshot,
  ) {
    this.id = raw.id;
    this.name = raw.name;
    this.timeJudge = new T3TimeWrapper(raw.timeJudge);
    this.timeEnd = new T3TimeWrapper(raw.timeEnd);
    this.isDummy = raw.isDummy;
  }

  get track(): TrackSnapshot {
    const track = this.chart.resolveTrack(this.raw.track);
    if (track === undefined) throw new Error("Track not found");
    return track;
  }

  getRaw(): any {
    return this.raw;
  }

  get timeMin(): T3Time {
    return new T3Time(this.raw.timeJudge.value);
  }
  get timeMax(): T3Time {
    return new T3Time(this.raw.timeEnd.value);
  }
  nudge(distance: T3Time): void {
    if (distance.milli > 0) {
      this.timeEnd.value = new T3Time(
        this.timeEnd.value.milli + distance.milli,
      );
      this.timeJudge.value = new T3Time(
        this.timeJudge.value.milli + distance.milli,
      );
    } else {
      this.timeJudge.value = new T3Time(
        this.timeJudge.value.milli + distance.milli,
      );
      this.timeEnd.value = new T3Time(
        this.timeEnd.value.milli + distance.milli,
      );
    }
  }
  getModel(): HoldModel {
    return new HoldModel(
      new T3Time(this.raw.timeJudge.value),
      new T3Time(this.raw.timeEnd.value),
      this.raw.isDummy.value,
    );
  }
}

export interface DraftNoteModel extends NoteModel {
  position: number;
  width: number;
}

interface RawDraftHitData {
  type: string;
  id: Wrapper<number>;
  name: Wrapper<string>;
  hitType: Wrapper<HitType>;
  timeJudge: Wrapper<number>;
  position: Wrapper<number>;
  width: Wrapper<number>;
  isDummy: Wrapper<boolean>;
}

interface RawDraftHoldData {
  type: string;
  id: Wrapper<number>;
  name: Wrapper<string>;
  timeJudge: Wrapper<number>;
  timeEnd: Wrapper<number>;
  position: Wrapper<number>;
  width: Wrapper<number>;
  isDummy: Wrapper<boolean>;
}

export class DraftHitModel extends HitModel implements DraftNoteModel {
  constructor(
    hitType: HitType,
    timeJudge: T3Time,
    private _position: number,
    private _width: number,
    isDummy: boolean = false,
  ) {
    super(hitType, timeJudge, isDummy);
  }

  get position(): number {
    return this._position;
  }
  set position(v: number) {
    this._position = v;
  }
  get width(): number {
    return this._width;
  }
  set width(v: number) {
    this._width = v;
  }

  toCSharp(): any {
    return new CS.MusicGame.Models.Note.DraftHit(
      new CS.T3Framework.Runtime.T3Time(this.timeJudge.milli),
      this.hitType,
      this.position,
      this.width,
    );
  }
}

export class DraftHoldModel extends HoldModel implements DraftNoteModel {
  constructor(
    timeJudge: T3Time,
    timeEnd: T3Time,
    private _position: number,
    private _width: number,
    isDummy: boolean = false,
  ) {
    super(timeJudge, timeEnd, isDummy);
  }

  get position(): number {
    return this._position;
  }
  set position(v: number) {
    this._position = v;
  }
  get width(): number {
    return this._width;
  }
  set width(v: number) {
    this._width = v;
  }

  toCSharp(): any {
    return new CS.MusicGame.Models.Note.DraftHold(
      new CS.T3Framework.Runtime.T3Time(this.timeJudge.milli),
      new CS.T3Framework.Runtime.T3Time(this.timeEnd.milli),
      this.position,
      this.width,
    );
  }
}

export class DraftHitSnapshot implements NoteSnapshot {
  readonly id: Wrapper<number>;
  readonly name: Wrapper<string>;
  readonly hitType: Wrapper<HitType>;
  readonly timeJudge: T3TimeWrapper;
  readonly position: Wrapper<number>;
  readonly width: Wrapper<number>;
  readonly isDummy: Wrapper<boolean>;

  constructor(
    private raw: RawDraftHitData,
    private chart: ChartSnapshot,
  ) {
    this.id = raw.id;
    this.name = raw.name;
    this.hitType = raw.hitType;
    this.timeJudge = new T3TimeWrapper(raw.timeJudge);
    this.position = raw.position;
    this.width = raw.width;
    this.isDummy = raw.isDummy;
  }

  getRaw(): any {
    return this.raw;
  }

  get timeMin(): T3Time {
    return new T3Time(this.raw.timeJudge.value);
  }
  get timeMax(): T3Time {
    return new T3Time(this.raw.timeJudge.value);
  }
  nudge(distance: T3Time): void {
    this.timeJudge.value = new T3Time(
      this.timeJudge.value.milli + distance.milli,
    );
  }
  getModel(): DraftHitModel {
    return new DraftHitModel(
      this.raw.hitType.value,
      new T3Time(this.raw.timeJudge.value),
      this.raw.position.value,
      this.raw.width.value,
      this.raw.isDummy.value,
    );
  }
}

export class DraftHoldSnapshot implements NoteSnapshot {
  readonly id: Wrapper<number>;
  readonly name: Wrapper<string>;
  readonly timeJudge: T3TimeWrapper;
  readonly timeEnd: T3TimeWrapper;
  readonly position: Wrapper<number>;
  readonly width: Wrapper<number>;
  readonly isDummy: Wrapper<boolean>;

  constructor(
    private raw: RawDraftHoldData,
    private chart: ChartSnapshot,
  ) {
    this.id = raw.id;
    this.name = raw.name;
    this.timeJudge = new T3TimeWrapper(raw.timeJudge);
    this.timeEnd = new T3TimeWrapper(raw.timeEnd);
    this.position = raw.position;
    this.width = raw.width;
    this.isDummy = raw.isDummy;
  }

  getRaw(): any {
    return this.raw;
  }

  get timeMin(): T3Time {
    return new T3Time(this.raw.timeJudge.value);
  }
  get timeMax(): T3Time {
    return new T3Time(this.raw.timeEnd.value);
  }
  nudge(distance: T3Time): void {
    this.timeJudge.value = new T3Time(
      this.timeJudge.value.milli + distance.milli,
    );
    this.timeEnd.value = new T3Time(this.timeEnd.value.milli + distance.milli);
  }
  getModel(): DraftHoldModel {
    return new DraftHoldModel(
      new T3Time(this.raw.timeJudge.value),
      new T3Time(this.raw.timeEnd.value),
      this.raw.position.value,
      this.raw.width.value,
      this.raw.isDummy.value,
    );
  }
}
