import type { ComponentSnapshot } from "../model.js";
import { createSnapshot, toArray, toCSharpObjectArray } from "./t3chart.js";
import type { ChartSnapshot, SnapshotRegistry } from "./t3chart.js";
import type { DraftNoteModel, NoteModel, NoteSnapshot } from "./t3notes.js";
import type { TrackModel, TrackSnapshot } from "./t3track.js";
import type { ClipboardApi } from "./t3context.js";

/** One entry of the clipboard, handed out as an independent copy on every read. */
export interface ClipboardItem {
  /**
   * The copied component. Editing it changes neither the clipboard nor the chart. A copied track hands out its
   * own copied notes through `component.notes`.
   */
  readonly component: ComponentSnapshot;

  /**
   * The track the copy was attached to, as a snapshot of `ctx.chart`. It is undefined for a copy that has no
   * track (a track copy, a draft note copy) or whose track left the chart after the copy was taken.
   */
  readonly parent?: TrackSnapshot;
}

/**
 * The content that replaces the clipboard. Create one of the three implementations below; one content only
 * accepts one kind (tracks, notes or draft notes), so the kinds can never be mixed in a single `override`.
 *
 * `_apply` is internal: `ChartClipboard.override` calls it, plugins never do.
 */
export interface ClipboardContent {
  _apply(api: ClipboardApi): boolean;
}

/** Whether the value looks like a model, which is what the add methods accept. */
function isModel(value: any): boolean {
  return (
    value !== null && value !== undefined && typeof value.toCSharp === "function"
  );
}

/** Track content. `addTrack` mirrors `ChartSnapshot.addTrack` without its callbacks. */
export class TrackClipboardContent implements ClipboardContent {
  private readonly entries: {
    track: TrackModel;
    notes: NoteModel[];
    layerId?: number;
  }[] = [];

  /**
   * Adds a track (and optionally its notes) to the content.
   *
   * @param layerId The id of the layer the track belongs to (see `ctx.chart.layersInfo.layers`). `undefined`
   *   keeps the default layer. Whether the id still exists is checked when the content is applied.
   * @returns Whether the arguments look like models. `false` means nothing was added to the content.
   */
  addTrack(
    model: TrackModel,
    notes: NoteModel[] = [],
    layerId?: number,
  ): boolean {
    if (!isModel(model)) return false;
    for (const note of notes) {
      if (!isModel(note)) return false;
    }

    this.entries.push({ track: model, notes, layerId });
    return true;
  }

  _apply(api: ClipboardApi): boolean {
    api.beginOverride();
    for (const entry of this.entries) {
      if (
        !api.addTrack(
          entry.track.toCSharp(),
          toCSharpObjectArray(entry.notes),
          entry.layerId ?? null,
        )
      ) {
        api.cancelOverride();
        return false;
      }
    }

    api.commitOverride();
    return true;
  }
}

/** Note content. `addNote` mirrors `ChartSnapshot.addNote` without its callback. */
export class NoteClipboardContent implements ClipboardContent {
  private readonly entries: { note: NoteModel; track: TrackSnapshot }[] = [];

  /**
   * Adds a note attached to the given track to the content.
   *
   * @param track A track of `ctx.chart`; it becomes the track the pasted note is attached to. A track that is
   *   not in the current chart is rejected when the content is applied.
   * @returns Whether the arguments look like models. `false` means nothing was added to the content.
   */
  addNote(model: NoteModel, track: TrackSnapshot): boolean {
    if (!isModel(model)) return false;
    if (track === null || track === undefined) return false;
    if (typeof track.getRaw !== "function") return false;

    this.entries.push({ note: model, track });
    return true;
  }

  _apply(api: ClipboardApi): boolean {
    api.beginOverride();
    for (const entry of this.entries) {
      if (!api.addNote(entry.note.toCSharp(), entry.track.getRaw())) {
        api.cancelOverride();
        return false;
      }
    }

    api.commitOverride();
    return true;
  }
}

/** Draft note content. `addDraftNote` mirrors `ChartSnapshot.addDraftNote` without its callback. */
export class DraftNoteClipboardContent implements ClipboardContent {
  private readonly entries: DraftNoteModel[] = [];

  /**
   * Adds a floating (draft) note to the content.
   *
   * @returns Whether the argument looks like a model. `false` means nothing was added to the content.
   */
  addDraftNote(model: DraftNoteModel): boolean {
    if (!isModel(model)) return false;

    this.entries.push(model);
    return true;
  }

  _apply(api: ClipboardApi): boolean {
    api.beginOverride();
    for (const entry of this.entries) {
      if (!api.addDraftNote(entry.toCSharp())) {
        api.cancelOverride();
        return false;
      }
    }

    api.commitOverride();
    return true;
  }
}

/**
 * The clipboard of the editor. Every read creates new copies, so editing what it handed out changes neither the
 * clipboard nor the chart; the only way to change the clipboard is `override`.
 */
export class ChartClipboard {
  constructor(
    private api: ClipboardApi,
    private chart: ChartSnapshot,
  ) {}

  /**
   * The clipboard content, in no particular order. It is read from C# and copied again on every access, so it is
   * cheaper to keep the returned array in a local variable than to read it repeatedly.
   */
  get items(): ReadonlyArray<ClipboardItem> {
    const registry = new ClipboardSnapshot(this.chart);
    return toArray(this.api.getAll()).map((raw: any) => ({
      component: registry.create(raw),
      parent:
        raw.type === "Hit" || raw.type === "Hold"
          ? this.chart.resolveTrack(raw.track)
          : undefined,
    }));
  }

  /**
   * Replaces the clipboard content with the given one, immediately and without touching the undo history, as a
   * single all or nothing operation: when anything is rejected (an unknown layer id, a track that is not in the
   * chart, a model of the wrong kind), the clipboard keeps its previous content and `false` is returned.
   *
   * An empty content clears the clipboard.
   */
  override(content: ClipboardContent): boolean {
    return content._apply(this.api);
  }
}

/**
 * Resolves the raw objects of one read. The copies are in no chart, so a raw the chart does not know is turned
 * into a snapshot of this read and cached; the cache is what keeps `note.track` the copied track itself.
 */
class ClipboardSnapshot implements SnapshotRegistry {
  private readonly noteByRaw = new Map<any, NoteSnapshot>();
  private readonly trackByRaw = new Map<any, TrackSnapshot>();

  constructor(private chart: SnapshotRegistry) {}

  create(raw: any): ComponentSnapshot {
    return raw.type === "Track"
      ? this.resolveTrack(raw)!
      : this.resolveNote(raw)!;
  }

  resolveNote(raw: any): NoteSnapshot | undefined {
    if (raw === null || raw === undefined || raw.type === "Track")
      return undefined;
    const existing = this.noteByRaw.get(raw) ?? this.chart.resolveNote(raw);
    if (existing !== undefined) return existing;

    const note = createSnapshot(raw, this) as NoteSnapshot;
    this.noteByRaw.set(raw, note);
    return note;
  }

  resolveTrack(raw: any): TrackSnapshot | undefined {
    if (raw === null || raw === undefined || raw.type !== "Track")
      return undefined;
    const existing = this.trackByRaw.get(raw) ?? this.chart.resolveTrack(raw);
    if (existing !== undefined) return existing;

    const track = createSnapshot(raw, this) as TrackSnapshot;
    this.trackByRaw.set(raw, track);
    return track;
  }
}
