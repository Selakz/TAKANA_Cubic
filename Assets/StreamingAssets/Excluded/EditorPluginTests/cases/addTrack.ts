import type { PluginTestHarness } from "../harness.js";
import { check, equal } from "../harness.js";
import { makeDraftHitModel, makeHitModel, makeTrackModel } from "../helpers.js";

/**
 * Scenarios covering ChartSnapshot.addTrack / addNote / addDraftNote / removeComponent.
 *
 * Callbacks are only reachable through closures, so the captured values are read into `const`s before being
 * narrowed; reading a closure-assigned `let` directly would make TypeScript lose its declared type.
 */
export function registerAddTrackCases(harness: PluginTestHarness): void {
  harness.scenario("addTrack.layer-and-callbacks", () => {
    const ctx = getT3Context();
    const chart = ctx.chart;
    const layerId = chart.layersInfo.defaultLayer.id;

    let track: TrackSnapshot | undefined;
    let notes: (NoteSnapshot | undefined)[] | undefined;
    let committed = false;

    const accepted = chart.addTrack(
      makeTrackModel(0, 2000),
      [makeHitModel(500)],
      layerId,
      (added) => {
        track = added;
        check(harness, committed, "track callback runs while committing, not when calling addTrack");
      },
      (added) => {
        notes = added;
      },
    );

    check(harness, accepted, "addTrack accepted the arguments");
    check(harness, track === undefined, "track callback not called before commit");
    check(harness, notes === undefined, "notes callback not called before commit");

    committed = true;
    ctx.commit();

    const addedTrack = track;
    const addedNotes = notes;
    check(harness, addedTrack !== undefined, "track callback called after commit");
    check(harness, addedNotes !== undefined, "notes callback called after commit");
    if (addedTrack === undefined || addedNotes === undefined) return;

    equal(harness, addedTrack.layer.id, layerId, "track belongs to the requested layer");
    equal(harness, addedNotes.length, 1, "notes callback array matches the input length");

    const note = addedNotes[0];
    if (note === undefined) {
      harness.fail("note snapshot received", "notes callback entry is undefined");
    } else {
      check(harness, note instanceof HitSnapshot, "note snapshot has the expected type");
      check(
        harness,
        (note as HitSnapshot).track === addedTrack,
        "note.track resolves to the same snapshot as the added track",
      );
      equal(harness, note.timeJudge.value.milli, 500, "note time preserved");
    }

    equal(harness, countOf(chart.tracks), 1, "chart.tracks updated after commit");
    equal(harness, countOf(chart.notes), 1, "chart.notes updated after commit");
  });

  harness.scenario("addTrack.rejects-invalid-arguments", () => {
    const ctx = getT3Context();
    const chart = ctx.chart;

    let rejectedTrack: TrackSnapshot | undefined;
    let rejectedNotes: (NoteSnapshot | undefined)[] | undefined;
    let trackCallbackCalled = false;

    const layerRejected = chart.addTrack(
      makeTrackModel(0, 1000),
      [makeHitModel(100)],
      999999,
      (added) => {
        trackCallbackCalled = true;
        rejectedTrack = added;
      },
      (added) => {
        rejectedNotes = added;
      },
    );

    check(harness, !layerRejected, "unknown layer id is rejected");
    check(harness, trackCallbackCalled, "the track callback is called even when rejected");
    check(harness, rejectedTrack === undefined, "the rejected track callback receives undefined");

    const rejectedNotesArray = rejectedNotes;
    check(harness, rejectedNotesArray !== undefined, "the notes callback is called even when rejected");
    if (rejectedNotesArray !== undefined) {
      equal(harness, rejectedNotesArray.length, 1, "the notes callback array keeps the input length");
      check(harness, rejectedNotesArray[0] === undefined, "rejected notes callback entries are undefined");
    }

    const modelRejected = chart.addTrack(makeHitModel(0) as unknown as TrackModel, []);
    check(harness, !modelRejected, "a non-track model is rejected");
    equal(harness, countOf(chart.tracks), 0, "nothing was added before commit");

    check(harness, chart.addTrack(makeTrackModel(0, 1000), []), "a valid call is still accepted");
    ctx.commit();
    equal(harness, countOf(chart.tracks), 1, "only the valid track was committed");
  });

  harness.scenario("addTrack.callback-runs-once", () => {
    const ctx = getT3Context();
    const chart = ctx.chart;

    let trackCalls = 0;
    let noteCalls = 0;
    chart.addTrack(
      makeTrackModel(0, 1000),
      [makeHitModel(100)],
      undefined,
      () => {
        trackCalls++;
      },
      () => {
        noteCalls++;
      },
    );
    ctx.commit();
    equal(harness, trackCalls, 1, "track callback called once by commit");
    equal(harness, noteCalls, 1, "notes callback called once by commit");

    harness.undo();
    equal(harness, countOf(chart.tracks), 0, "undo removed the track");
    harness.redo();
    equal(harness, countOf(chart.tracks), 1, "redo restored the track");
    equal(harness, trackCalls, 1, "track callback not replayed by redo");
    equal(harness, noteCalls, 1, "notes callback not replayed by redo");
  });

  harness.scenario("addNote-and-addDraftNote.callbacks", () => {
    const ctx = getT3Context();
    const chart = ctx.chart;

    let track: TrackSnapshot | undefined;
    chart.addTrack(makeTrackModel(0, 2000), [], undefined, (added) => {
      track = added;
    });
    ctx.commit();
    const addedTrack = track;
    check(harness, addedTrack !== undefined, "track added");
    if (addedTrack === undefined) return;

    let note: NoteSnapshot | undefined;
    let draftNote: NoteSnapshot | undefined;
    const noteAccepted = chart.addNote(makeHitModel(500), addedTrack, (added) => {
      note = added;
    });
    const draftAccepted = chart.addDraftNote(makeDraftHitModel(800), (added) => {
      draftNote = added;
    });

    check(harness, noteAccepted && draftAccepted, "addNote and addDraftNote accepted");
    check(harness, note === undefined && draftNote === undefined, "callbacks not called before commit");

    ctx.commit();
    const addedNote = note;
    const addedDraftNote = draftNote;
    check(harness, addedNote !== undefined, "addNote callback ran after commit");
    check(harness, addedDraftNote !== undefined, "addDraftNote callback ran after commit");
    if (addedNote !== undefined) {
      check(
        harness,
        (addedNote as HitSnapshot).track === addedTrack,
        "note.track resolves to the same snapshot as the added track",
      );
    }

    let rejectedNote: NoteSnapshot | undefined;
    let rejectedCallbackCalled = false;
    const rejected = chart.addNote(
      makeHitModel(900),
      addedDraftNote as unknown as TrackSnapshot,
      (added) => {
        rejectedCallbackCalled = true;
        rejectedNote = added;
      },
    );
    check(harness, !rejected, "a note snapshot is not accepted as the target track");
    check(harness, rejectedCallbackCalled, "the note callback is called even when rejected");
    check(harness, rejectedNote === undefined, "the rejected note callback receives undefined");
    equal(harness, countOf(chart.notes), 2, "the rejected call added nothing");
  });

  harness.scenario("removeComponent.staged-removal", () => {
    const ctx = getT3Context();
    const chart = ctx.chart;

    let track: TrackSnapshot | undefined;
    let note: NoteSnapshot | undefined;
    chart.addTrack(
      makeTrackModel(0, 1000),
      [makeHitModel(100)],
      undefined,
      (added) => {
        track = added;
      },
      (added) => {
        note = added[0];
      },
    );
    ctx.commit();

    const addedTrack = track;
    const addedNote = note;
    check(harness, addedTrack !== undefined, "track snapshot received");
    check(harness, addedNote !== undefined, "note snapshot received");
    if (addedTrack === undefined || addedNote === undefined) return;

    equal(harness, countOf(chart.tracks), 1, "one track before removal");
    equal(harness, countOf(chart.notes), 1, "one note before removal");

    chart.removeComponent(addedNote);
    equal(harness, countOf(chart.notes), 1, "removal is staged until commit");
    ctx.commit();
    equal(harness, countOf(chart.notes), 0, "note removed after commit");

    chart.removeComponent(addedTrack);
    ctx.commit();
    equal(harness, countOf(chart.tracks), 0, "track removed after commit");
  });
}

function countOf<T>(set: ReadonlySet<T>): number {
  let count = 0;
  for (const value of set) count++;
  return count;
}
