/**
 * The clipboard of track movement nodes, used by the editor's node copy/paste (see `ctx.nodes`).
 *
 * Every read assembles new copies, so editing what it handed out changes neither the clipboard nor the chart;
 * the only way to change the clipboard is `override`.
 */
declare class NodeClipboard {
  /**
   * The clipboard content as a single movement: a `TrackEdgeMovement` when the clipboard holds left/right nodes,
   * a `TrackDirectMovement` when it holds position/width nodes. An empty clipboard reads as a `TrackEdgeMovement`
   * whose two `MoveList`s have no node.
   *
   * It is assembled from the editor on every access, so keep it in a local variable instead of reading it
   * repeatedly. When the editor's clipboard mixes both groups, only the left/right nodes are read, so writing the
   * result back drops the position/width nodes.
   */
  readonly movement: TrackMovement;

  /**
   * Replaces the clipboard content with the given movement, immediately and without touching the undo history
   * (no `ctx.commit()` needed).
   *
   * A `TrackEdgeMovement` writes left/right nodes, a `TrackDirectMovement` writes position/width nodes; both can
   * be pasted by the editor right away. Returns whether the movement was accepted. Anything else (a wrapper, a
   * plain object) is rejected and the clipboard keeps its previous content. An empty movement clears the
   * clipboard.
   */
  override(movement: TrackMovement): boolean;
}
