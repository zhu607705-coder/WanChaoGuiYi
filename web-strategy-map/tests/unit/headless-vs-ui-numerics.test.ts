import { describe, expect, it } from 'vitest';
import { describeHeadlessReport, parseHeadlessReport } from './headless-report-helpers';

/**
 * Bug under investigation: tools/headless_runner/latest-war-report.json
 * is the source-of-truth for headless verification. The Playwright UI
 * test reads UI text and clicks buttons, asserting human-readable
 * values like "进攻方 700"，"占领后整合度=25%". If the headless calc
 * and the UI display drift, neither side notices: headless owns the
 * canonical numbers, UI owns the rendering, and there's no joint
 * test that both produce the same numbers from the same scenario.
 *
 * Pinned invariant (lightweight today): the headless report should
 * include explicit numeric keyDeltas, and a future cross-check
 * harness can compare them against UI snapshots. We pin the
 * immediate prerequisite: every keyDelta value must be a primitive
 * (number / string / boolean), not an object — otherwise UI cannot
 * render it directly.
 */
describe('headless war report contract', () => {
  describeHeadlessReport('real latest-war-report.json', (report) => {
    it('every keyDelta has primitive before/after values', () => {
      for (const sc of report.scenarios) {
        const deltas = sc.keyDeltas ?? [];
        for (const d of deltas) {
          const primitive = (v: unknown) =>
            v === null ||
            typeof v === 'string' ||
            typeof v === 'number' ||
            typeof v === 'boolean';
          expect.soft(
            primitive(d.before),
            `scenario ${sc.name} delta ${d.field}: before=${JSON.stringify(d.before)}`
          ).toBe(true);
          expect.soft(
            primitive(d.after),
            `scenario ${sc.name} delta ${d.field}: after=${JSON.stringify(d.after)}`
          ).toBe(true);
        }
      }
    });

    it('every scenario has at least one numeric keyDelta', () => {
      for (const sc of report.scenarios) {
        const deltas = sc.keyDeltas ?? [];
        const hasNumeric = deltas.some(
          (d: { before: unknown; after: unknown }) =>
            typeof d.before === 'number' || typeof d.after === 'number'
        );
        expect.soft(hasNumeric, `scenario ${sc.name} has no numeric keyDelta`).toBe(true);
      }
    });
  });

  it('rejects empty or malformed report fixtures', () => {
    expect(() => parseHeadlessReport('[]')).toThrow('report root must be an object');
    expect(() => parseHeadlessReport('{"scenarios":[]}')).toThrow('scenarios must be a non-empty array');
    expect(() => parseHeadlessReport('{"scenarios":[{"name":"bad","keyDeltas":"oops"}]}')).toThrow(
      'scenario bad keyDeltas must be an array'
    );
  });
});
