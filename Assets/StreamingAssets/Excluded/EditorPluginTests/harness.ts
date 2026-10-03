/**
 * Harness handed to the test plugin's `runTests(harness)` entry.
 *
 * Before every scenario the runner assigns a fresh chart to the editor level, re-initializes the bridge and then
 * invokes the scenario body, so `getT3Context()` inside a body always points at a brand new chart.
 *
 * Scenario bodies must run synchronously: there is no frame pumping while a scenario runs.
 */
export interface PluginTestHarness {
  /** Runs one scenario on a fresh chart of the test level. The C# side checks of the same name run afterwards. */
  scenario(name: string, body: () => void): void;

  /** Records one assertion result. */
  check(ok: boolean, name: string, detail: string): void;

  /** Records a failure without a condition. */
  fail(name: string, detail: string): void;

  /** Records an informational line in the report. */
  log(message: string): void;

  /** Undoes the last commit of the current scenario. */
  undo(): void;

  /** Redoes the last undone commit of the current scenario. */
  redo(): void;
}

/** Convenience wrapper that kills non boolean conditions. */
export function check(
  harness: PluginTestHarness,
  ok: boolean,
  name: string,
  detail: string = "",
): void {
  harness.check(!!ok, name, detail);
}

/** Convenience wrapper comparing two values with strict equality. */
export function equal(
  harness: PluginTestHarness,
  actual: unknown,
  expected: unknown,
  name: string,
): void {
  const format = (value: unknown): string =>
    value === undefined ? "undefined" : value === null ? "null" : String(value);
  harness.check(
    actual === expected,
    name,
    `actual: ${format(actual)}, expected: ${format(expected)}`,
  );
}
