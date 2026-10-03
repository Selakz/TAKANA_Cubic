import type { PluginTestHarness } from "../harness.js";
import { check, equal } from "../harness.js";
import { makeHitModel, makeTrackModel } from "../helpers.js";

/** Scenarios covering the remaining chart, layer, bpm list and plugin base APIs. */
export function registerChartBasicCases(harness: PluginTestHarness): void {
  harness.scenario("layers.edit-and-track-setLayer", () => {
    const ctx = getT3Context();
    const chart = ctx.chart;
    const layersInfo = chart.layersInfo;
    const defaultId = layersInfo.defaultLayer.id;

    const added = layersInfo.add({
      name: "Test Layer",
      color: { r: 1, g: 0, b: 0, a: 1 },
      isDecoration: false,
      isSelected: true,
    });
    check(harness, added, "layer added");
    equal(harness, layersInfo.layers.length, 2, "two layers after add");

    const layer = layersInfo.layers.find((candidate) => candidate.name === "Test Layer");
    check(harness, layer !== undefined, "added layer is listed");
    if (layer === undefined) return;
    check(harness, layer.id !== defaultId, "added layer id differs from the default one");

    const updated = layersInfo.update(layer.id, {
      name: "Renamed Layer",
      color: { r: 0, g: 1, b: 0, a: 1 },
      isDecoration: false,
      isSelected: false,
    });
    check(harness, updated, "layer updated");
    equal(
      harness,
      layersInfo.layers.find((candidate) => candidate.id === layer.id)?.name,
      "Renamed Layer",
      "layer name updated",
    );

    let track: TrackSnapshot | undefined;
    chart.addTrack(makeTrackModel(0, 1000), [], layer.id, (added) => {
      track = added;
    });
    ctx.commit();

    const addedTrack = track;
    check(harness, addedTrack !== undefined, "track added on the new layer");
    if (addedTrack !== undefined) {
      equal(harness, addedTrack.layer.id, layer.id, "track belongs to the requested layer");
    }

    check(harness, layersInfo.remove(layer.id), "layer removed");
    equal(harness, layersInfo.layers.length, 1, "one layer left after remove");
  });

  harness.scenario("bpmList.edit", () => {
    const ctx = getT3Context();
    const bpmList = ctx.chart.bpmList;

    bpmList.clear();
    equal(harness, bpmList.size, 1, "clear keeps a single default node");

    const time = new T3Time(1000);
    bpmList.set(time, 150);
    equal(harness, bpmList.get(time), 150, "bpm value stored");
    check(harness, bpmList.has(time), "bpm node present");
    equal(harness, bpmList.size, 2, "bpm list size grew");

    const clamped = new T3Time(2000);
    bpmList.set(clamped, 0.5);
    equal(harness, bpmList.get(clamped), 1, "bpm value clamped to 1");

    const floor = bpmList.getFloorTime(new T3Time(1500), 4);
    const ceil = bpmList.getCeilTime(new T3Time(1500), 4);
    check(harness, floor.milli <= 1500, "floor time is not after the given time");
    check(harness, ceil.milli >= 1500, "ceil time is not before the given time");

    check(harness, bpmList.delete(time), "bpm node deleted");
    check(harness, !bpmList.has(time), "bpm node gone");
    equal(harness, bpmList.size, 2, "bpm list size shrank");
  });

  harness.scenario("params.defaults-and-writes", () => {
    equal(harness, params.get("testCount")!.value, 0, "int param default");
    equal(harness, params.get("testFlag")!.value, false, "bool param default");
    equal(harness, params.get("testText")!.value, "", "string param default");

    params.get("testCount")!.value = 7;
    equal(harness, params.get("testCount")!.value, 7, "int param write round trip");
    params.get("testFlag")!.value = true;
    equal(harness, params.get("testFlag")!.value, true, "bool param write round trip");
    params.get("testText")!.value = "hello";
    equal(harness, params.get("testText")!.value, "hello", "string param write round trip");
  });

  harness.scenario("pluginBase.events", () => {
    const ctx = getT3Context();
    const chart = ctx.chart;

    let notesAdded = 0;
    let tracksAdded = 0;

    class Probe extends T3PluginBase {
      protected onNoteAdded(note: NoteSnapshot): void {
        notesAdded++;
      }

      protected onTrackAdded(track: TrackSnapshot): void {
        tracksAdded++;
      }
    }

    const probe = new Probe();
    check(harness, probe !== undefined, "probe plugin base constructed");

    chart.addTrack(makeTrackModel(0, 1000), [makeHitModel(100)], undefined);
    ctx.commit();

    equal(harness, tracksAdded, 1, "onTrackAdded fired once");
    equal(harness, notesAdded, 1, "onNoteAdded fired once");
  });
}
