/**
 * One entry of the editor clipboard.
 *
 * Reading the clipboard hands out independent copies: editing them changes neither the clipboard nor the chart.
 */
interface ClipboardItem {
  /**
   * The copied component, i.e. a `HitSnapshot` / `HoldSnapshot` / `DraftHitSnapshot` / `DraftHoldSnapshot` /
   * `TrackSnapshot`.
   *
   * A copied track hands out its own copied notes through `component.notes`, and those notes resolve their
   * `track` back to that very copy. The copies belong to no chart: they never appear in `ctx.chart.tracks` /
   * `ctx.chart.notes`, and a copied track resolves `component.layer` through the current chart by the layer id
   * stored in the copy (`Track has no layer info.` when that layer was removed meanwhile).
   */
  readonly component: ComponentSnapshot;

  /**
   * The track the copy was attached to, as a snapshot of `ctx.chart` (one of `ctx.chart.tracks`).
   *
   * It is `undefined` for a copy without a track (a track copy or a draft note copy), and for a note copy whose
   * track has left the chart after the copy was taken; in the latter case `component.track` throws.
   */
  readonly parent?: TrackSnapshot;
}

/**
 * The content that replaces the clipboard, built with one of `TrackClipboardContent`,
 * `NoteClipboardContent` or `DraftNoteClipboardContent`.
 *
 * One content only accepts one kind (tracks, notes or draft notes), corresponding to the component kinds the
 * editor's copy/paste separates, so a single `override` can never mix them.
 */
interface ClipboardContent {}

/**
 * The clipboard of the editor, used by copy/paste.
 *
 * Every read creates new copies, so editing what it handed out changes neither the clipboard nor the chart;
 * the only way to change the clipboard is `override`.
 */
declare class ChartClipboard {
  /**
   * The clipboard content, in no particular order.
   *
   * It is read from the editor and copied again on every access, so keep the returned array in a local variable
   * instead of reading it repeatedly.
   */
  readonly items: ReadonlyArray<ClipboardItem>;

  /**
   * Replaces the clipboard content with the given one, immediately and without touching the undo history (no
   * `ctx.commit()` needed).
   *
   * The replacement is all or nothing: when anything is rejected (an unknown layer id, a track that is not in
   * `ctx.chart`, a model of the wrong kind), the clipboard keeps its previous content and `false` is returned.
   * An empty content clears the clipboard.
   */
  override(content: ClipboardContent): boolean;
}

/**
 * Track content; `addTrack` mirrors `ChartSnapshot.addTrack` without its callbacks.
 *
 * The pasted track is attached under the judge line, exactly like a track added with `ctx.chart.addTrack`.
 */
declare class TrackClipboardContent implements ClipboardContent {
  /**
   * Adds a track, and optionally its notes, to the content.
   *
   * @param layerId The id of the layer the track belongs to (see `layersInfo.layers`).
   *   `undefined` keeps the default layer. A non-existent id is rejected by `override`.
   * @returns Whether the arguments are models. `false` means nothing was added to the content.
   */
  addTrack(model: TrackModel, notes?: NoteModel[], layerId?: number): boolean;
}

/** Note content; `addNote` mirrors `ChartSnapshot.addNote` without its callback. */
declare class NoteClipboardContent implements ClipboardContent {
  /**
   * Adds a note attached to the given track to the content.
   *
   * @param track A track of `ctx.chart`; it becomes the track the pasted note is attached to. A track that is
   *   not in the current chart (`ctx.chart.tracks`) is rejected by `override`.
   * @returns Whether the arguments are models. `false` means nothing was added to the content.
   */
  addNote(model: NoteModel, track: TrackSnapshot): boolean;
}

/** Draft note content; `addDraftNote` mirrors `ChartSnapshot.addDraftNote` without its callback. */
declare class DraftNoteClipboardContent implements ClipboardContent {
  /**
   * Adds a floating (draft) note to the content.
   *
   * @returns Whether the argument is a model. `false` means nothing was added to the content.
   */
  addDraftNote(model: DraftNoteModel): boolean;
}
