import { T3Time } from "../model.js";
import {
  createMoveItem,
  MoveList,
  TrackEdgeMovement,
  TrackDirectMovement,
} from "./t3track.js";
import type { TrackMovement } from "./t3track.js";
import { toArray } from "./t3chart.js";
import type { NodeClipboardApi } from "./t3context.js";

/**
 * The clipboard of track movement nodes, used by the editor's node copy/paste. Every read assembles new
 * copies, so editing what it handed out changes neither the clipboard nor the chart; the only way to change the
 * clipboard is `override`.
 */
export class NodeClipboard {
  constructor(private api: NodeClipboardApi) {}

  /**
   * The clipboard content as one movement, assembled from the editor on every access: the edge nodes come back
   * as a `TrackEdgeMovement`, the position/width nodes as a `TrackDirectMovement`. An empty clipboard reads as
   * an edge movement without nodes, so callers never have to handle `undefined`.
   *
   * It is assembled from value snapshots, so editing it changes neither the clipboard nor the chart.
   */
  get movement(): TrackMovement {
    const nodes: any[] = toArray(this.api.readNodes());

    const moveList = (type: string): MoveList => {
      const list = new MoveList();
      for (const raw of nodes) {
        if (raw.type !== type) continue;
        list.set(new T3Time(raw.time), createMoveItem(raw.getMoveItem()));
      }
      return list;
    };

    // An empty clipboard reads as an edge movement without nodes.
    const isEdge =
      nodes.length === 0 ||
      nodes.some((raw: any) => raw.type === "Left" || raw.type === "Right");
    return isEdge
      ? new TrackEdgeMovement(moveList("Left"), moveList("Right"))
      : new TrackDirectMovement(moveList("Pos"), moveList("Width"));
  }

  /**
   * Replaces the clipboard content with the given movement, immediately and without touching the undo history:
   * a `TrackEdgeMovement` writes left/right nodes, a `TrackDirectMovement` writes position/width nodes.
   *
   * Returns whether the movement was accepted. Anything else leaves the clipboard untouched, and an empty
   * movement clears it.
   */
  override(movement: TrackMovement): boolean {
    const model: any = (movement as any)?.toCSharp?.();
    if (model === null || model === undefined) return false;
    return this.api.overrideMovement(model);
  }
}
