import type { PluginTestHarness } from "../harness.js";
import { check, equal } from "../harness.js";
import { makeHitModel, makeTrackModel } from "../helpers.js";

/** Scenarios covering the chart clipboard API. */
export function registerChartClipboardCases(harness: PluginTestHarness): void {
  harness.scenario("chartClipboard.override-and-read", () => {
    const ctx = getT3Context();
    const chart = ctx.chart;

    // A track of its own, so the scenario can prove that the clipboard content stays out of the chart.
    chart.addTrack(makeTrackModel(0, 1000), [makeHitModel(100)]);
    ctx.commit();
    equal(harness, chart.tracks.size, 1, "the chart has its own track");

    const content = new TrackClipboardContent();
    check(
      harness,
      content.addTrack(makeTrackModel(2000, 3000), [makeHitModel(2100)]),
      "the content accepts a track",
    );
    check(harness, ctx.chartClipboard.override(content), "the override is accepted");

    const items = ctx.chartClipboard.items;
    equal(harness, items.length, 1, "one clipboard item");
    const item = items[0];
    const trackCopy = item.component as TrackSnapshot;
    check(harness, trackCopy instanceof TrackSnapshot, "the copy is a track snapshot");
    equal(harness, item.parent, undefined, "a track copy has no parent");
    equal(harness, trackCopy.timeMin.milli, 2000, "the copy keeps the source start time");
    equal(harness, trackCopy.timeMax.milli, 3000, "the copy keeps the source end time");
    equal(harness, chart.tracks.size, 1, "the clipboard content never enters the chart");

    const notes = Array.from(trackCopy.notes);
    equal(harness, notes.length, 1, "the copied track hands out its copied note");
    const note = notes[0];
    check(harness, note instanceof HitSnapshot && note.track === trackCopy, "the copied note is a hit snapshot which resolves its own track copy");
    equal(harness, note.timeJudge.value.milli, 2100, "the copied note keeps its source time");

    // Copies are independent of the clipboard: edits on them do not survive the next read.
    trackCopy.nudge(new T3Time(500));
    note.nudge(new T3Time(50));
    const reread = ctx.chartClipboard.items[0].component as TrackSnapshot;
    equal(harness, reread.timeMin.milli, 2000, "editing a read copy does not change the clipboard");
    equal(
      harness,
      Array.from(reread.notes)[0].timeJudge.value.milli,
      2100,
      "editing a copied note does not change the clipboard",
    );
    const chartTrack = Array.from(chart.tracks)[0];
    equal(harness, chartTrack.timeMin.milli, 0, "the chart's own track is untouched");
    equal(harness, chart.notes.size, 1, "the chart's own note is untouched");

    // The layer id is validated when the content is applied.
    const badLayer = new TrackClipboardContent();
    check(
      harness,
      badLayer.addTrack(makeTrackModel(0, 100), [], 999999),
      "the content accepts an unknown layer id",
    );
    check(harness, !ctx.chartClipboard.override(badLayer), "an unknown layer id rejects the override");
    equal(harness, ctx.chartClipboard.items.length, 1, "a rejected override keeps the previous content");

    check(
      harness,
      ctx.chartClipboard.override(new NoteClipboardContent()),
      "the empty content is accepted",
    );
    equal(harness, ctx.chartClipboard.items.length, 0, "the empty content clears the clipboard");

    // Leave a known content behind: the C# checks of this scenario read the clipboard after the body ran.
    const finalContent = new TrackClipboardContent();
    finalContent.addTrack(makeTrackModel(2000, 3000), [makeHitModel(2100)]);
    check(harness, ctx.chartClipboard.override(finalContent), "the last override is accepted");
    equal(harness, ctx.chartClipboard.items.length, 1, "the last content is the only clipboard item");
  });

  harness.scenario("chartClipboard.note-copy-and-deleted-parent", () => {
    const ctx = getT3Context();
    const chart = ctx.chart;

    let track: TrackSnapshot | undefined;
    chart.addTrack(makeTrackModel(0, 1000), [], undefined, (added) => {
      track = added;
    });
    ctx.commit();
    const chartTrack = track;
    if (chartTrack === undefined) {
      harness.fail("setup", "the track was not added to the chart");
      return;
    }

    const content = new NoteClipboardContent();
    check(harness, content.addNote(makeHitModel(500), chartTrack), "the content accepts a note");
    check(harness, ctx.chartClipboard.override(content), "the override is accepted");

    const item = ctx.chartClipboard.items[0];
    const noteCopy = item.component as HitSnapshot;
    check(harness, noteCopy instanceof HitSnapshot, "the copy is a hit snapshot");
    equal(harness, item.parent === chartTrack, true, "the parent is the track of the chart");
    equal(harness, noteCopy.track === chartTrack, true, "the copy resolves the track of the chart");
    equal(harness, chart.notes.size, 0, "the copied note never enters the chart");

    const replacement = new NoteClipboardContent();
    check(harness, replacement.addNote(makeHitModel(600), chartTrack), "the content accepts the replacement");
    check(harness, ctx.chartClipboard.override(replacement), "the replacement is accepted");
    equal(harness, ctx.chartClipboard.items.length, 1, "the previous content is gone");
    equal(
      harness,
      (ctx.chartClipboard.items[0].component as HitSnapshot).timeJudge.value.milli,
      600,
      "the replacement content is the current one",
    );

    chart.removeComponent(chartTrack);
    ctx.commit();
    equal(harness, chart.tracks.size, 0, "the track left the chart");
    const orphan = ctx.chartClipboard.items[0];
    equal(harness, orphan.parent, undefined, "the parent is gone with its track");
    let trackThrew = false;
    try {
      void (orphan.component as HitSnapshot).track;
    } catch {
      trackThrew = true;
    }
    check(harness, trackThrew, "component.track throws when its track left the chart");

    const invalid = new NoteClipboardContent();
    check(
      harness,
      invalid.addNote(makeHitModel(700), chartTrack),
      "the content accepts the detached track",
    );
    check(harness, !ctx.chartClipboard.override(invalid), "a detached track rejects the override");
    equal(
      harness,
      (ctx.chartClipboard.items[0].component as HitSnapshot).timeJudge.value.milli,
      600,
      "the rejected override keeps the previous content",
    );
  });
}
