import {
  HitType,
  HitModel,
  HitSnapshot,
  HoldModel,
  HoldSnapshot,
  DraftHitModel,
  DraftHoldModel,
  DraftHitSnapshot,
  DraftHoldSnapshot,
} from "./t3notes.js";
import {
  Eases,
  MoveList,
  EaseMoveItem,
  BezierMoveItem,
  TrackEdgeMovement,
  TrackDirectMovement,
  TrackModel,
  TrackSnapshot,
  TrackEdgeMovementWrapper,
  TrackDirectMovementWrapper,
} from "./t3track.js";
import { TrackEdgeNode, TrackDirectNode } from "./t3nodes.js";
import { T3CSharpApi, T3Context, createContext } from "./t3context.js";
import { T3PluginBase } from "./t3pluginbase.js";
import { EmptyWrapper } from "../model.js";

const emptyChartApi = {
  offsetMilli: 0,
  bpmList: {
    getFloorTime: (t: number, d: number) => t,
    getCeilTime: (t: number, d: number) => t,
    has: () => false,
    get: () => null,
    delete: () => false,
    clear: () => {},
    set: () => {},
    keys: () => [],
    values: () => [],
    size: 0,
  },
  layersInfo: {
    layers: [],
    defaultLayer: null,
    add: () => false,
    remove: () => false,
    update: () => false,
  },
  onNoteAdded: () => {},
  onNoteRemoved: () => {},
  getAllNotes: () => [],
  onTrackAdded: () => {},
  onTrackRemoved: () => {},
  getAllTracks: () => [],
  getAllSelected: () => [],
  getCurrentSelecting: () => undefined,
  addSelected: () => {},
  removeSelected: () => {},
  clearSelected: () => {},
  addTrack: () => {},
  addNote: () => {},
  addDraftNote: () => {},
  removeComponent: () => {},
};

const emptyApi: T3CSharpApi = {
  chart: emptyChartApi,
  staging: {
    hasPending: false,
    commit: () => {},
  },
  editor: {
    chartTime: new EmptyWrapper(0),
    audioLengthMilli: 0,

    showHeader: () => {},
    showConfirm: () => {},
    showConfirmAndCancel: () => {},
  },
  nodes: {
    getAllNodes: () => [],
    onNodeAdded: () => {},
    onNodeRemoved: () => {},
    getAllSelected: () => [],
    getCurrentSelecting: () => undefined,
    addSelected: () => {},
    removeSelected: () => {},
    clearSelected: () => {},
  },
  mouse: {
    getTimeStart: () => undefined,
    getHoldTimeEnd: () => undefined,
    getTrackTimeEnd: () => undefined,
    getWidth: () => undefined,
    getPosition: () => undefined,
    getAttachedPosition: () => undefined,
  },
  loadChart: () => undefined,
  createNewChart: () => emptyChartApi,
  saveChart: () => false,
};

// t3note.ts
Object.freeze(HitType);
globalThis.HitType = HitType;
globalThis.HitModel = HitModel;
globalThis.HoldModel = HoldModel;
globalThis.HitSnapshot = HitSnapshot;
globalThis.HoldSnapshot = HoldSnapshot;
globalThis.DraftHitModel = DraftHitModel;
globalThis.DraftHoldModel = DraftHoldModel;
globalThis.DraftHitSnapshot = DraftHitSnapshot;
globalThis.DraftHoldSnapshot = DraftHoldSnapshot;

// t3track.ts
globalThis.Eases = Eases;
globalThis.MoveList = MoveList;
globalThis.EaseMoveItem = EaseMoveItem;
globalThis.BezierMoveItem = BezierMoveItem;
globalThis.TrackEdgeMovement = TrackEdgeMovement;
globalThis.TrackEdgeMovementWrapper = TrackEdgeMovementWrapper;
globalThis.TrackDirectMovement = TrackDirectMovement;
globalThis.TrackDirectMovementWrapper = TrackDirectMovementWrapper;
globalThis.TrackModel = TrackModel;
globalThis.TrackSnapshot = TrackSnapshot;

// t3nodes.ts
globalThis.TrackEdgeNode = TrackEdgeNode;
globalThis.TrackDirectNode = TrackDirectNode;

// t3pluginbase.ts
globalThis.T3PluginBase = T3PluginBase;

let stubbedCtx: T3Context | null = null;
globalThis.getT3Context = (): T3Context => {
  if (stubbedCtx === null) {
    stubbedCtx = createContext(emptyApi);
  }
  return stubbedCtx!;
};

export function __t3_bridge_init(api: T3CSharpApi): void {
  const ctx = createContext(api);
  globalThis.getT3Context = () => ctx;
}
