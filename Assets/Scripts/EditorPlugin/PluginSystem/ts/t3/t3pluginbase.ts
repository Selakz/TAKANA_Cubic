import type { T3Context } from "./t3context.js";
import type { NoteSnapshot } from "./t3notes.js";
import type { TrackSnapshot } from "./t3track.js";
import type { TrackNode } from "./t3nodes.js";

export abstract class T3PluginBase {
  private readonly ctx: T3Context;

  constructor() {
    this.ctx = getT3Context();
    this.ctx.chart._onNoteAdded((note) => this.onNoteAdded(note));
    this.ctx.chart._onNoteRemoved((note) => this.onNoteRemoved(note));
    this.ctx.chart._onTrackAdded((track) => this.onTrackAdded(track));
    this.ctx.chart._onTrackRemoved((track) => this.onTrackRemoved(track));
    this.ctx.nodes._onNodeAdded((node) => this.onNodeAdded(node));
    this.ctx.nodes._onNodeRemoved((node) => this.onNodeRemoved(node));
  }

  protected onNoteAdded(note: NoteSnapshot): void {}

  protected onNoteRemoved(note: NoteSnapshot): void {}

  protected onTrackAdded(track: TrackSnapshot): void {}

  protected onTrackRemoved(track: TrackSnapshot): void {}

  protected onNodeAdded(node: TrackNode): void {}

  protected onNodeRemoved(node: TrackNode): void {}
}
