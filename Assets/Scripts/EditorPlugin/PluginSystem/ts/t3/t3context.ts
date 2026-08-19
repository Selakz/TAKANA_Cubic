import {
  Wrapper,
  T3Time,
  T3TimeWrapper,
  I18NString,
  ComponentSnapshot,
} from "../model.js";
import { ChartSnapshot, ChartSelectSet } from "./t3chart.js";
import { NodeDataset, NodeSelectSet } from "./t3nodes.js";
import type { TrackNode } from "./t3nodes.js";

export interface SelectSet<T> extends Set<T> {
  readonly currentSelecting: T | undefined;
}

export interface MouseInfoRetriever {
  getTimeStart(): T3Time | undefined;
  getHoldTimeEnd(): T3Time | undefined;
  getTrackTimeEnd(): T3Time | undefined;

  getWidth(): number | undefined;
  getPosition(): number | undefined;
  getAttachedPosition(): number | undefined;
}

export interface ChartApi {
  readonly offsetMilli: number;
  readonly bpmList: any;
  readonly layersInfo: any;
  onNoteAdded(callback: (raw: any) => void): void;
  onNoteRemoved(callback: (raw: any) => void): void;
  getAllNotes(): any[];
  onTrackAdded(callback: (raw: any) => void): void;
  onTrackRemoved(callback: (raw: any) => void): void;
  getAllTracks(): any[];
  getAllSelected(): any[];
  getCurrentSelecting(): any;
  addSelected(raw: any): void;
  removeSelected(raw: any): void;
  clearSelected(): void;
  addTrack(model: any, noteModels: any[]): void;
  addNote(model: any, track: any): void;
  addDraftNote(model: any): void;
  removeComponent(raw: any): void;
}

export interface StagingApi {
  readonly hasPending: boolean;
  commit(): void;
}

export interface EditorApi {
  readonly chartTime: Wrapper<number>;
  readonly audioLengthMilli: number;

  showHeader(content: any, logType: number): void;
  showConfirm(content: any, callback: () => void): void;
  showConfirmAndCancel(content: any, callback: (choice: number) => void): void;
}

export interface NodeApi {
  getAllNodes(): any[];
  onNodeAdded(callback: (raw: any) => void): void;
  onNodeRemoved(callback: (raw: any) => void): void;
  getAllSelected(): any[];
  getCurrentSelecting(): any;
  addSelected(raw: any): void;
  removeSelected(raw: any): void;
  clearSelected(): void;
}

export interface MouseApi {
  getTimeStart(): number | undefined;
  getHoldTimeEnd(): number | undefined;
  getTrackTimeEnd(): number | undefined;
  getWidth(): number | undefined;
  getPosition(): number | undefined;
  getAttachedPosition(): number | undefined;
}

export interface T3CSharpApi {
  readonly chart: ChartApi;
  readonly staging: StagingApi;
  readonly editor: EditorApi;
  readonly nodes: NodeApi;
  readonly mouse: MouseApi;

  loadChart(path: string): any;
  createNewChart(): any;
  saveChart(path: string, chart: any): boolean;
}

export interface T3Context {
  readonly chart: ChartSnapshot;
  readonly chartSelectDataset: SelectSet<ComponentSnapshot>;
  readonly chartTime: Wrapper<T3Time>;
  readonly audioLength: T3Time;

  readonly nodes: NodeDataset;
  readonly nodeSelectDataset: SelectSet<TrackNode>;
  readonly mouseInfoRetriever: MouseInfoRetriever;

  showHeader(content: I18NString, logType: number): void;
  showConfirm(content: I18NString, callback: () => void): void;
  showConfirmAndCancel(
    content: I18NString,
    callback: (choice: number) => void,
  ): void;

  loadChart(path: string): ChartSnapshot | undefined;
  createNewChart(): ChartSnapshot;
  saveChart(path: string, chart: ChartSnapshot): boolean;

  commit(): void;
}

export function createContext(api: T3CSharpApi): T3Context {
  return new T3ContextImpl(api);
}

class T3ContextImpl implements T3Context {
  readonly chart: ChartSnapshot;
  readonly chartTime: Wrapper<T3Time>;
  readonly chartSelectDataset: ChartSelectSet;
  readonly nodes: NodeDataset;
  readonly nodeSelectDataset: NodeSelectSet;
  readonly mouseInfoRetriever: MouseInfoRetriever;

  constructor(private api: T3CSharpApi) {
    this.chart = new ChartSnapshot(api.chart);
    this.chartTime = new T3TimeWrapper(api.editor.chartTime);
    this.chartSelectDataset = new ChartSelectSet(api.chart, this.chart);
    this.nodes = new NodeDataset(api.nodes, this.chart);
    this.nodeSelectDataset = new NodeSelectSet(api.nodes, this.nodes);
    this.mouseInfoRetriever = new MouseInfoRetrieverImpl(api.mouse);
  }

  get audioLength(): T3Time {
    return new T3Time(this.api.editor.audioLengthMilli);
  }

  showHeader(content: I18NString, logType: number) {
    this.api.editor.showHeader(this.buildI18NString(content), logType);
  }

  showConfirm(content: I18NString, callback: () => void) {
    this.api.editor.showConfirm(this.buildI18NString(content), callback);
  }

  showConfirmAndCancel(
    content: I18NString,
    callback: (choice: number) => void,
  ) {
    this.api.editor.showConfirmAndCancel(
      this.buildI18NString(content),
      callback,
    );
  }

  loadChart(path: string): ChartSnapshot | undefined {
    const api = this.api.loadChart(path);
    if (api === null || api === undefined) return undefined;
    return new ChartSnapshot(api);
  }

  createNewChart(): ChartSnapshot {
    return new ChartSnapshot(this.api.createNewChart());
  }

  saveChart(path: string, chart: ChartSnapshot): boolean {
    return this.api.saveChart(path, chart.getChartApi());
  }

  commit(): void {
    this.api.staging.commit();
  }

  private buildI18NString(content: I18NString): any {
    var CSharpI18NString = CS.T3Framework.Runtime.I18N.I18NString;
    var Language = CS.T3Framework.Runtime.I18N.Language;
    var i18nString = new CSharpI18NString();
    if (content.en) i18nString.Add(Language.English, content.en);
    if (content.zh_Hans)
      i18nString.Add(Language.SimplifiedChinese, content.zh_Hans);
    if (content.zh_Hant)
      i18nString.Add(Language.TraditionalChinese, content.zh_Hant);
    if (content.ja) i18nString.Add(Language.Japanese, content.ja);
    return i18nString;
  }
}

class MouseInfoRetrieverImpl implements MouseInfoRetriever {
  constructor(private api: MouseApi) {}

  getTimeStart(): T3Time | undefined {
    const milli = this.api.getTimeStart();
    return milli === null || milli === undefined ? undefined : new T3Time(milli);
  }
  getHoldTimeEnd(): T3Time | undefined {
    const milli = this.api.getHoldTimeEnd();
    return milli === null || milli === undefined ? undefined : new T3Time(milli);
  }
  getTrackTimeEnd(): T3Time | undefined {
    const milli = this.api.getTrackTimeEnd();
    return milli === null || milli === undefined ? undefined : new T3Time(milli);
  }
  getWidth(): number | undefined {
    const width = this.api.getWidth();
    return width === null || width === undefined ? undefined : width;
  }
  getPosition(): number | undefined {
    const position = this.api.getPosition();
    return position === null || position === undefined ? undefined : position;
  }
  getAttachedPosition(): number | undefined {
    const position = this.api.getAttachedPosition();
    return position === null || position === undefined ? undefined : position;
  }
}
