import type { PluginTestHarness } from "./harness.js";
import { registerAddTrackCases } from "./cases/addTrack.js";
import { registerChartBasicCases } from "./cases/chartBasics.js";
import { registerChartClipboardCases } from "./cases/chartClipboard.js";
import { registerNodeClipboardCases } from "./cases/nodeClipboard.js";

/**
 * Test plugin used by PluginTestRunner (Tools/Editor Plugin/Run System Tests).
 *
 * The runner assigns a test level to the editor and loads this module through the production plugin path, so the
 * bridge talks to the level's chart. Before every scenario it swaps in a fresh chart, re-initializes the bridge and
 * only then calls runTests. Two consequences:
 * - scenario bodies must stay synchronous;
 * - getT3Context() must be called *inside* a scenario body, because the runner replaces the global context before
 *   every scenario. A context captured at module load time or in an earlier scenario is already stale.
 */
const plugin = {
  /** Unused here: the runner calls runTests instead. Kept so the plugin also loads in the editor panel. */
  execute: (): void => {},

  runTests: (harness: PluginTestHarness): void => {
    registerAddTrackCases(harness);
    registerChartBasicCases(harness);
    registerChartClipboardCases(harness);
    registerNodeClipboardCases(harness);
  },
};

export default plugin;
