import type { PluginTestHarness } from "../harness.js";
import { check, equal } from "../harness.js";

/** Scenarios covering the node clipboard API. */
export function registerNodeClipboardCases(harness: PluginTestHarness): void {
  harness.scenario("nodeClipboard.override-and-read", () => {
    const ctx = getT3Context();

    // An empty movement clears the clipboard, which makes the read deterministic even though the clipboard
    // survives the chart swap between scenarios.
    check(
      harness,
      ctx.nodeClipboard.override(new TrackEdgeMovement(new MoveList(), new MoveList())),
      "an empty movement is accepted",
    );
    const empty = ctx.nodeClipboard.movement;
    check(harness, empty instanceof TrackEdgeMovement, "an empty clipboard reads as an edge movement");
    equal(harness, (empty as TrackEdgeMovement).leftMoveList.items.size, 0, "an empty clipboard has no left node");
    equal(harness, (empty as TrackEdgeMovement).rightMoveList.items.size, 0, "an empty clipboard has no right node");
    equal(harness, (empty as TrackEdgeMovement).getPosition(new T3Time(0)), 0, "an empty movement can still be queried");

    const edge = new TrackEdgeMovement(new MoveList(), new MoveList());
    edge.leftMoveList.set(new T3Time(100), new EaseMoveItem(-2, Eases.Linear));
    edge.leftMoveList.set(
      new T3Time(300),
      new BezierMoveItem(-4, 0.25, 0.25, 0.75, 0.75),
    );
    edge.rightMoveList.set(new T3Time(100), new EaseMoveItem(2, Eases.Unmove));
    edge.rightMoveList.set(new T3Time(300), new EaseMoveItem(4, Eases.Unmove));
    check(harness, ctx.nodeClipboard.override(edge), "the edge movement is accepted");

    const read = ctx.nodeClipboard.movement;
    check(
      harness,
      read instanceof TrackEdgeMovement,
      "an edge clipboard reads back as an edge movement",
    );
    const edgeCopy = read as TrackEdgeMovement;
    equal(harness, edgeCopy.leftMoveList.items.size, 2, "the read keeps both left nodes");
    equal(harness, edgeCopy.rightMoveList.items.size, 2, "the read keeps both right nodes");
    equal(harness, edgeCopy.getLeftPosition(new T3Time(100)), -2, "the read keeps the left position");
    equal(harness, edgeCopy.getRightPosition(new T3Time(100)), 2, "the read keeps the right position");
    equal(harness, edgeCopy.getPosition(new T3Time(100)), 0, "the read keeps the movement position");
    equal(harness, edgeCopy.getWidth(new T3Time(100)), 4, "the read keeps the movement width");
    const items = Array.from(edgeCopy.leftMoveList.items.values());
    check(
      harness,
      items.some((item) => item instanceof BezierMoveItem && item.position === -4),
      "a bezier node keeps its type",
    );

    // The read is a copy: neither editing it nor editing the movement it was written from touches the clipboard.
    edgeCopy.shift(100);
    edgeCopy.nudge(new T3Time(50));
    edgeCopy.leftMoveList.set(new T3Time(100), new EaseMoveItem(-99, Eases.Linear));
    edge.leftMoveList.set(new T3Time(100), new EaseMoveItem(-98, Eases.Linear));
    const reread = ctx.nodeClipboard.movement as TrackEdgeMovement;
    equal(harness, reread.leftMoveList.items.size, 2, "the times of a read copy do not change the clipboard");
    equal(harness, reread.getLeftPosition(new T3Time(100)), -2, "editing a read copy does not change the clipboard");
    equal(harness, reread.getWidth(new T3Time(100)), 4, "editing the written movement does not change the clipboard");

    const direct = new TrackDirectMovement(new MoveList(), new MoveList());
    direct.positionMoveList.set(new T3Time(200), new EaseMoveItem(1, Eases.Linear));
    direct.widthMoveList.set(new T3Time(200), new EaseMoveItem(4, Eases.Linear));
    check(harness, ctx.nodeClipboard.override(direct), "the direct movement is accepted");
    const readDirect = ctx.nodeClipboard.movement;
    check(
      harness,
      readDirect instanceof TrackDirectMovement,
      "a direct clipboard reads back as a direct movement",
    );
    equal(harness, (readDirect as TrackDirectMovement).getPosition(new T3Time(200)), 1, "the read keeps the position node");
    equal(harness, (readDirect as TrackDirectMovement).getWidth(new T3Time(200)), 4, "the read keeps the width node");

    check(harness, !ctx.nodeClipboard.override({} as any), "a plain object is rejected");
    check(harness, !ctx.nodeClipboard.override(undefined as any), "undefined is rejected");
    check(
      harness,
      ctx.nodeClipboard.movement instanceof TrackDirectMovement,
      "a rejected override keeps the previous content",
    );

    // Leave a known content behind: the C# checks of this scenario read the clipboard after the body ran.
    const finalContent = new TrackEdgeMovement(new MoveList(), new MoveList());
    finalContent.leftMoveList.set(new T3Time(100), new EaseMoveItem(-2, Eases.Linear));
    finalContent.leftMoveList.set(
      new T3Time(300),
      new BezierMoveItem(-4, 0.25, 0.25, 0.75, 0.75),
    );
    finalContent.rightMoveList.set(new T3Time(100), new EaseMoveItem(2, Eases.Unmove));
    finalContent.rightMoveList.set(new T3Time(300), new EaseMoveItem(4, Eases.Unmove));
    check(harness, ctx.nodeClipboard.override(finalContent), "the last override is accepted");
    equal(
      harness,
      (ctx.nodeClipboard.movement as TrackEdgeMovement).leftMoveList.items.size,
      2,
      "the last content is the only clipboard content",
    );
  });
}
